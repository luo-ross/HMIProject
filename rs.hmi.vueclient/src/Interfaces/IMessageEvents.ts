import type { MessageEnum } from "../Commons/Enums/MessageEnum";
export interface IMessageEvents {
  ShowDangerMsg: (msg: string | null) => void;
  ShowDarkMsg: (msg: string | null) => void;
  ShowDismissibleMsg: (msg: string | null) => void;
  ShowHeadingMsg: (msg: string | null) => void;
  ShowInfoMsg: (msg: string | null) => void;
  ShowLightMsg: (msg: string | null) => void;
  ShowLinkMsg: (msg: string | null) => void;
  ShowPrimaryMsg: (msg: string | null) => void;
  ShowSecondaryMsg: (msg: string | null) => void;
  ShowSuccessMsg: (msg: string | null) => void;
  ShowWarningMsg: (msg: string|null) => void;
  ShowMsg: (type: MessageEnum, msg: string) => void;
  ClearMsg: () => void;
}    
