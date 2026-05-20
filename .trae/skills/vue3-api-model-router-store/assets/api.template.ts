import axios from 'axios'
import type {
  __ModulePascal__CreateRequestDto,
  __ModulePascal__Dto,
  __ModulePascal__ListQueryDto,
  __ModulePascal__ListResponseDto,
  __ModulePascal__UpdateRequestDto,
} from './__module__.types'

const baseUrl = '/api/__module__'

export async function list__ModulePascal__(query: __ModulePascal__ListQueryDto): Promise<__ModulePascal__ListResponseDto> {
  const { data } = await axios.get<__ModulePascal__ListResponseDto>(baseUrl, { params: query })
  return data
}

export async function get__ModulePascal__ById(id: string): Promise<__ModulePascal__Dto> {
  const { data } = await axios.get<__ModulePascal__Dto>(`${baseUrl}/${id}`)
  return data
}

export async function create__ModulePascal__(payload: __ModulePascal__CreateRequestDto): Promise<__ModulePascal__Dto> {
  const { data } = await axios.post<__ModulePascal__Dto>(baseUrl, payload)
  return data
}

export async function update__ModulePascal__(id: string, payload: __ModulePascal__UpdateRequestDto): Promise<__ModulePascal__Dto> {
  const { data } = await axios.put<__ModulePascal__Dto>(`${baseUrl}/${id}`, payload)
  return data
}

export async function delete__ModulePascal__(id: string): Promise<void> {
  await axios.delete(`${baseUrl}/${id}`)
}
