import fs from 'node:fs/promises';
import path from 'node:path';

const inputPath = path.resolve('.devin/wiki.json');
const outputRoot = path.resolve('public/AutoCodeForge.wiki');

const toArray = (value) => {
  if (Array.isArray(value)) return value;
  if (!value) return [];
  if (typeof value === 'object') return Object.values(value);
  return [];
};

const slugify = (value) =>
  String(value || '')
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9]+/g, '-')
    .replace(/^-+|-+$/g, '') || 'untitled';

const asText = (value, fallback = '') => {
  if (value === null || value === undefined || value === '') return fallback;
  if (typeof value === 'string') return value;
  return JSON.stringify(value, null, 2);
};

const normalizePages = (repo, repoRefFallback = '') => {
  const rawPages = toArray(repo.pages ?? repo.wikiPages ?? repo.documents ?? repo.items);
  return rawPages.map((p, index) => {
    const title = asText(p.title, asText(p.name, 'Untitled'));
    const slug = slugify(p.slug ?? title);
    const order = Number.isFinite(Number(p.order)) ? Number(p.order) : index + 1;
    const toc = Array.isArray(p.toc) ? p.toc : [];
    return {
      title,
      slug,
      parent: asText(p.parent, ''),
      repoName: asText(p.repoName, asText(repo.repoName, asText(repo.name, 'unknown-repo'))),
      ref: asText(p.ref, asText(repo.ref, repoRefFallback || '')),
      author: asText(p.author, ''),
      updatedAt: asText(p.updatedAt, ''),
      path: asText(p.path, ''),
      purpose: asText(p.purpose, ''),
      notes: p.notes,
      order,
      toc,
    };
  });
};

const renderNotes = (notes) => {
  if (Array.isArray(notes)) {
    if (notes.length === 0) return '_No notes provided._';
    return notes.map((n) => `- ${asText(n)}`).join('\n');
  }
  const text = asText(notes, '');
  return text || '_No notes provided._';
};

const markdownForPage = (page) => {
  const purpose = page.purpose || '_No purpose provided._';
  const notes = renderNotes(page.notes);
  return [
    `# ${page.title}`,
    '',
    '| Key | Value |',
    '| --- | --- |',
    `| repo | ${page.repoName} |`,
    `| ref | ${page.ref || ''} |`,
    `| author | ${page.author || ''} |`,
    `| updatedAt | ${page.updatedAt || ''} |`,
    `| path | ${page.path || ''} |`,
    `| parent | ${page.parent || ''} |`,
    '',
    '## Purpose',
    '',
    purpose,
    '',
    '## Notes',
    '',
    notes,
    '',
  ].join('\n');
};

const normalizeParent = (value) => {
  const text = asText(value, '').trim();
  if (!text || text === '根目录') return '';
  return text;
};

const buildHierarchy = (pages) => {
  const titleMap = new Map(pages.map((page) => [page.title, page]));

  const topForPage = (page) => {
    let current = page;
    const visited = new Set();
    while (current.parent) {
      if (visited.has(current.title)) break;
      visited.add(current.title);
      const parent = titleMap.get(current.parent);
      if (!parent) break;
      current = parent;
    }
    return current;
  };

  const topPages = pages.filter((page) => !page.parent || !titleMap.has(page.parent));

  const childrenMap = new Map();
  for (const top of topPages) {
    childrenMap.set(top.title, []);
  }

  for (const page of pages) {
    const top = topForPage(page);
    const current = childrenMap.get(top.title) ?? [];
    if (page.title !== top.title) {
      current.push(page);
    }
    childrenMap.set(top.title, current);
  }

  return { topPages, childrenMap, topForPage };
};

const main = async () => {
  const raw = await fs.readFile(inputPath, 'utf8');
  const data = JSON.parse(raw);

  await fs.mkdir(outputRoot, { recursive: true });

  const repoCandidates = [];
  if (data.repoName) {
    repoCandidates.push({
      repoName: data.repoName,
      ref: data.ref ?? '',
      pages: data.pages ?? data.wikiPages ?? [],
    });
  }
  for (const r of toArray(data.repositories)) {
    repoCandidates.push({
      repoName: r.repoName ?? r.name ?? 'unknown-repo',
      ref: r.ref ?? '',
      pages: r.pages ?? r.wikiPages ?? r.documents ?? r.items ?? [],
    });
  }

  const seen = new Set();
  const repos = [];
  for (const repo of repoCandidates) {
    const repoName = asText(repo.repoName, 'unknown-repo');
    const key = `${repoName}@@${asText(repo.ref, '')}`;
    if (seen.has(key)) continue;
    seen.add(key);
    repos.push({ ...repo, repoName });
  }

  const manifest = {
    generatedAt: new Date().toISOString(),
    repos: [],
  };

  const rootOrder = [];
  let mdCount = 0;

  for (const repo of repos) {
    const repoDir = path.join(outputRoot, repo.repoName);
    await fs.mkdir(repoDir, { recursive: true });
    rootOrder.push(repo.repoName);

    const pages = normalizePages(repo, data.ref ?? '')
      .map((page) => ({ ...page, parent: normalizeParent(page.parent) }))
      .sort((a, b) => a.order - b.order);

    const { topPages, childrenMap, topForPage } = buildHierarchy(pages);
    const repoOrderLines = [];
    const manifestPages = [];

    for (const topPage of topPages) {
      const topFileName = `${topPage.slug}.md`;
      const topFilePath = path.join(repoDir, topFileName);
      await fs.writeFile(topFilePath, markdownForPage(topPage), 'utf8');
      repoOrderLines.push(topPage.slug);
      mdCount += 1;

      manifestPages.push({
        title: topPage.title,
        slug: topPage.slug,
        repoName: topPage.repoName,
        ref: topPage.ref,
        parent: '',
        topTitle: topPage.title,
        level: 1,
        path: `/${topPage.slug}`,
        filePath: path.relative(outputRoot, topFilePath).replace(/\\/g, '/'),
        order: topPage.order,
        toc: topPage.toc,
      });

      const children = (childrenMap.get(topPage.title) ?? []).sort((a, b) => a.order - b.order);
      if (children.length > 0) {
        const topFolder = path.join(repoDir, topPage.slug);
        await fs.mkdir(topFolder, { recursive: true });
        const childOrderLines = [];

        for (const childPage of children) {
          const childFileName = `${childPage.slug}.md`;
          const childFilePath = path.join(topFolder, childFileName);
          await fs.writeFile(childFilePath, markdownForPage(childPage), 'utf8');
          childOrderLines.push(childPage.slug);
          mdCount += 1;

          manifestPages.push({
            title: childPage.title,
            slug: childPage.slug,
            repoName: childPage.repoName,
            ref: childPage.ref,
            parent: childPage.parent,
            topTitle: topPage.title,
            level: 2,
            path: `/${topPage.slug}/${childPage.slug}`,
            filePath: path.relative(outputRoot, childFilePath).replace(/\\/g, '/'),
            order: childPage.order,
            toc: childPage.toc,
          });
        }

        await fs.writeFile(path.join(topFolder, '.order'), `${childOrderLines.join('\n')}\n`, 'utf8');
      }
    }

    await fs.writeFile(path.join(repoDir, '.order'), `${repoOrderLines.join('\n')}\n`, 'utf8');

    manifest.repos.push({
      repoName: repo.repoName,
      ref: asText(repo.ref, ''),
      pages: manifestPages,
    });
  }

  await fs.writeFile(path.join(outputRoot, '.order'), `${rootOrder.join('\n')}\n`, 'utf8');

  const manifestPath = path.join(outputRoot, 'manifest.json');
  await fs.writeFile(manifestPath, `${JSON.stringify(manifest, null, 2)}\n`, 'utf8');

  const manifestLines = (await fs.readFile(manifestPath, 'utf8')).split(/\r?\n/).slice(0, 15);

  console.log(`Repos: ${manifest.repos.length}`);
  console.log(`Markdown files generated: ${mdCount}`);
  console.log('Manifest (first 15 lines):');
  for (const line of manifestLines) console.log(line);
};

main().catch((err) => {
  console.error(err);
  process.exit(1);
});
