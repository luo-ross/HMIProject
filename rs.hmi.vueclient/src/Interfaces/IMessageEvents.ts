import type { MessageEnum } from "../Commons/Enums/MessageEnum";
export interface IMessageEvents {
  ShowDangerMsg: (msg: string) => void;
  ShowDarkMsg: (msg: string) => void;
  ShowDismissibleMsg: (msg: string) => void;
  ShowHeadingMsg: (msg: string) => void;
  ShowInfoMsg: (msg: string) => void;
  ShowLightMsg: (msg: string) => void;
  ShowLinkMsg: (msg: string) => void;
  ShowPrimaryMsg: (msg: string) => void;
  ShowSecondaryMsg: (msg: string) => void;
  ShowSuccessMsg: (msg: string) => void;
  ShowWarningMsg: (msg: string) => void;
  ShowMsg: (type: MessageEnum, msg: string) => void;
  ClearMsg: () => void;
}    
