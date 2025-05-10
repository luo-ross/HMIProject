import type { ILoadingEvents } from '../Interfaces/ILoadingEvents';
import type { IMessageEvents } from '../Interfaces/IMessageEvents';
import { ref } from 'vue';
import { ModelBase } from './ModelBase';

export abstract class ViewModelBase extends ModelBase {
  public LoadingEvents = ref<ILoadingEvents | null>(null);
  public MessageEvents = ref<IMessageEvents | null>(null);
 

  constructor() {
    super();
  }

  //public get LoadingEvents(): ILoadingEvents | null{
  //  return this._LoadingEvents.value
  //}
  //public set LoadingEvents(value: ILoadingEvents | null) {
  //  this._LoadingEvents.value = value;
  //}

  //public get MessageEvents(): IMessageEvents | null {
  //  return this._MessageEvents.value
  //}
  //public set MessageEvents(value: IMessageEvents | null) {
  //  this._MessageEvents.value = value;
  //}
}
