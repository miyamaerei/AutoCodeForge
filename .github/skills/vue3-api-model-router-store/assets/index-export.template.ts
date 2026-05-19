export { __module__Routes } from './routes'

export { use__ModulePascal__Store } from './store/use__ModulePascal__Store'

export {
  list__ModulePascal__,
  get__ModulePascal__ById,
  create__ModulePascal__,
  update__ModulePascal__,
  delete__ModulePascal__,
} from './api/__module__.api'

export type {
  __ModulePascal__Dto,
  __ModulePascal__ListQueryDto,
  __ModulePascal__ListResponseDto,
  __ModulePascal__CreateRequestDto,
  __ModulePascal__UpdateRequestDto,
} from './api/__module__.types'

export type { __ModulePascal__Model } from './models/__module__.model'

export { dtoToModel, modelToDto } from './models/__module__.mapper'
