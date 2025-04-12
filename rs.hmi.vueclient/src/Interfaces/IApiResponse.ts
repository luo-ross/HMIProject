export interface IApiResponse<T = unknown> {
  isSuccess: boolean;
  data: T;
  message: string;
}
