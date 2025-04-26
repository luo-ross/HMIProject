/**
 * 矩形框
 */
export class RectModel {
  public Left: number;
  public Top: number;
  public Width: number;
  public Height: number;
  constructor(left: number, top: number, width: number, height: number) {
    this.Left = left;
    this.Top = top;
    this.Width = width;
    this.Height = height;
  }
}
