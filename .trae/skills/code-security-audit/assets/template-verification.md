## 需求验收报告

**需求描述**: __demand_description__
**验收时间**: __checktime__
**验收状态**: __overall_status__

---

### L1 - 需求理解与范围界定

| 检查项 | 状态 | 证据 |
|--------|------|------|
| 需求拆解 | __l1_requirement_status__ | __l1_requirement_evidence__ |
| 边界定义 | __l1_boundary_status__ | __l1_boundary_evidence__ |
| 依赖识别 | __l1_dependency_status__ | __l1_dependency_evidence__ |
| 验收标准 | __l1_criteria_status__ | __l1_criteria_evidence__ |

**L1 Verdict**: __l1_verdict__

---

### L2 - 实现存在性验证

| 检查项 | 状态 | 证据 |
|--------|------|------|
| 代码存在 | __l2_file_status__ | __l2_file_evidence__ |
| 功能实现 | __l2_implementation_status__ | `__l2_implementation_evidence__` |
| 接口定义 | __l2_interface_status__ | `__l2_interface_evidence__` |
| 依赖注入 | __l2_di_status__ | `__l2_di_evidence__` |

**L2 Verdict**: __l2_verdict__

---

### L3 - 实现正确性验证

| 检查项 | 状态 | 证据 |
|--------|------|------|
| 逻辑正确性 | __l3_logic_status__ | __l3_logic_evidence__ |
| 数据流完整 | __l3_dataflow_status__ | __l3_dataflow_evidence__ |
| 边界处理 | __l3_boundary_status__ | `__l3_boundary_evidence__` |
| 调用链路 | __l3_callchain_status__ | __l3_callchain_evidence__ |

**L3 Verdict**: __l3_verdict__

---

### L4 - 质量与安全性验证

| 检查项 | 状态 | 证据 |
|--------|------|------|
| 安全检查 | __l4_security_status__ | __l4_security_evidence__ |
| 代码质量 | __l4_quality_status__ | __l4_quality_evidence__ |
| 测试覆盖 | __l4_test_status__ | __l4_test_evidence__ |
| 文档完整 | __l4_docs_status__ | __l4_docs_evidence__ |

**L4 Verdict**: __l4_verdict__

---

### 综合评估

| 层级 | 状态 |
|------|------|
| L1 | __l1_verdict__ |
| L2 | __l2_verdict__ |
| L3 | __l3_verdict__ |
| L4 | __l4_verdict__ |

**整体状态**: __overall_status__

**问题汇总**:
__issues_summary__

**建议**:
__recommendations__

---

**证据存档**: `.autoCodeForge/