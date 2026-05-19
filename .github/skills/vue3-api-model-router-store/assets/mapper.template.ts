import type { __ModulePascal__Dto } from '../api/__module__.types'
import type { __ModulePascal__Model } from './__module__.model'

export function dtoToModel(dto: __ModulePascal__Dto): __ModulePascal__Model {
  return {
    id: dto.id,
    name: dto.name ?? '',
    isActive: dto.isActive ?? false,
    createdAt: dto.createdAt ?? '',
  }
}

export function modelToDto(model: __ModulePascal__Model): __ModulePascal__Dto {
  return {
    id: model.id,
    name: model.name,
    isActive: model.isActive,
    createdAt: model.createdAt,
  }
}
