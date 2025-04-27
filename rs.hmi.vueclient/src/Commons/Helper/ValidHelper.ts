export class ValidHelper {


  /**
   * 判断字符串是否为空
   */
  public static IsEmptyOrNull(value: unknown): boolean {
    if (value === null || value === undefined) return true;
    if (typeof value === 'string') return value.trim() === '';
    return false;
  }


  /**
    * 验证传入的字符串是否为有效的邮箱地址
    * @param value - 待验证的邮箱地址字符串
    * @param isRestrict - 是否使用严格模式进行验证，默认为 false
    * @returns 如果是有效的邮箱地址返回 true，否则返回 false
    */
  public static IsEmail(value: string, isRestrict: boolean = true): boolean {
    // 检查传入的字符串是否为空或 undefined，如果是则直接判定为无效邮箱地址
    if (value === null || value === undefined || value.length === 0) {
      return false;
    }

    // 根据 isRestrict 参数选择不同的正则表达式模式
    const pattern = isRestrict
      // 严格模式下的正则表达式，支持带引号的用户名和更严格的域名格式
      ? /^(?:"([^"]+)"|([\w!#$%&'*+/=?^`{|}~-]+(?:\.[\w!#$%&'*+/=?^`{|}~-]+)*))@((?:[\w](?:[\w-]*[\w])?\.)+[a-zA-Z]{2,6})$/i
      // 非严格模式下的正则表达式，只要求基本的邮箱格式
      : /^[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)+$/i;

    // 使用正则表达式的 test 方法检查传入的字符串是否匹配邮箱模式
    return pattern.test(value);
  }


  /**
   * 是否合法的手机号码
   * @param value 手机号码
   * @returns 是否有效
   */
  public static IsPhoneNumber(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^(0|86|17951)?(13[0-9]|15[012356789]|18[0-9]|14[57]|17[678])[0-9]{8}$/.test(value);
  }

  /**
   * 是否手机号码
   * @param value 手机号码
   * @param isRestrict 是否按严格模式验证
   * @returns 是否有效
   */
  public static IsMobileNumberSimple(value: string, isRestrict: boolean = false): boolean {
    if (!value) {
      return false;
    }
    const pattern = isRestrict ? /^[1][3-8]\d{9}$/ : /^[1]\d{10}$/;
    return pattern.test(value);
  }

  /**
   * 是否手机号码
   * @param value 手机号码
   * @returns 是否有效
   */
  public static IsMobileNumber(value: string): boolean {
    if (!value) {
      return false;
    }
    value = value.trim().replace(/^/, '').replace(/\$/, '');
    return /^1(3[0-9]|4[57]|5[0-35-9]|8[0-9]|70)\d{8}$/.test(value);
  }

  /**
   * 是否中国移动号码
   * @param value 手机号码
   * @returns 是否有效
   */
  public static IsChinaMobilePhone(value: string): boolean {
    if (!value) {
      return false;
    }
    return /(^1(3[4-9]|4[7]|5[0-27-9]|7[8]|8[2-478])\d{8}$)|(^1705\d{7}$)/.test(value);
  }

  /**
   * 是否中国联通号码
   * @param value 手机号码
   * @returns 是否有效
   */
  public static IsChinaUnicomPhone(value: string): boolean {
    if (!value) {
      return false;
    }
    return /(^1(3[0-2]|4[5]|5[56]|7[6]|8[56])\d{8}$)|(^1709\d{7}$)/.test(value);
  }

  /**
   * 是否中国电信号码
   * @param value 手机号码
   * @returns 是否有效
   */
  public static IsChinaTelecomPhone(value: string): boolean {
    if (!value) {
      return false;
    }
    return /(^1(33|53|77|8[019])\d{8}$)|(^1700\d{7}$)/.test(value);
  }

  /**
   * 是否身份证号码
   * @param value 身份证
   * @returns 是否有效
   */
  public static IsIdCard(value: string): boolean {
    if (!value) {
      return false;
    }
    if (value.length === 15) {
      return /^[1-9]\d{7}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])\d{3}$/.test(value);
    }
    return value.length === 18 &&
      /^[1-9]\d{5}[1-9]\d{3}((0\d)|(1[0-2]))(([0|1|2]\d)|3[0-1])((\d{4})|\d{3}[Xx])$/.test(value);
  }

  /**
   * 是否Base64编码
   * @param value Base64字符串
   * @returns 是否有效
   */
  public static IsBase64String(value: string): boolean {
    return /[A-Za-z0-9\+\/\=]/.test(value);
  }

  /**
   * 是否日期
   * @param value 日期字符串
   * @param isRegex 是否正则验证
   * @returns 是否有效
   */
  public static IsDate(value: string, isRegex: boolean = false): boolean {
    if (!value) {
      return false;
    }
    if (isRegex) {
      return /^((((1[6-9]|[2-9]\d)\d{2})-(0?[13578]|1[02])-(0?[1-9]|[12]\d|3[01]))|(((1[6-9]|[2-9]\d)\d{2})-(0?[13456789]|1[012])-(0?[1-9]|[12]\d|30))|(((1[6-9]|[2-9]\d)\d{2})-0?2-(0?[1-9]|1\d|2[0-8]))|(((1[6-9]|[2-9]\d)(0[48]|[2468][048]|[13579][26])|((16|[2468][048]|[3579][26])00))-0?2-29-)) (20|21|22|23|[0-1]?\d):[0-5]?\d:[0-5]?\d$/.test(value);
    }
    return !isNaN(Date.parse(value));
  }

  /**
   * 是否Url地址
   * @param value url地址
   * @returns 是否有效
   */
  public static IsUrl(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^(http|https)\:\/\/([a-zA-Z0-9\.\-]+(\:[a-zA-Z0-9\.&%\$\-]+)*@)*((25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9])\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[1-9]|0)\.(25[0-5]|2[0-4][0-9]|[0-1]{1}[0-9]{2}|[1-9]{1}[0-9]{1}|[0-9])|localhost|([a-zA-Z0-9\-]+\.)*[a-zA-Z0-9\-]+\.(com|edu|gov|int|mil|net|org|biz|arpa|info|name|pro|aero|coop|museum|[a-zA-Z]{1,10}))(\:[0-9]+)*(\/($|[a-zA-Z0-9\.\,\?\'\\\+&%\$#\=~_\-]+))*$/.test(value);
  }

  /**
   * 是否Guid
   * @param value Guid字符串
   * @returns 是否有效
   */
  public static IsGuid(value: string): boolean {
    if (!value) {
      return false;
    }
    return /[A-F0-9]{8}(-[A-F0-9]{4}){3}-[A-F0-9]{12}|[A-F0-9]{32}/i.test(value);
  }

  /**
   * 是否大于0的正整数
   * @param value 正整数
   * @returns 是否有效
   */
  public static IsPositiveInteger(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^[1-9]+\d*$/.test(value);
  }

  /**
   * 是否Int32类型
   * @param value 整数
   * @returns 是否有效
   */
  public static IsInt32(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^[0-9]*$/.test(value);
  }

  /**
   * 是否Double类型
   * @param value 小数
   * @returns 是否有效
   */
  public static IsDouble(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^\d[.]?\d?$/.test(value);
  }

  /**
   * 是否整数
   * @param value 值
   * @returns 是否有效
   */
  public static IsInteger(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^\-?[0-9]+$/.test(value);
  }

  /**
   * 是否数字型
   * @param value 数字
   * @returns 是否有效
   */
  public static IsDecimal(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^([0-9])[0-9]*(\.\w*)?$/.test(value);
  }

  /**
   * 是否Mac地址
   * @param value Mac地址
   * @returns 是否有效
   */
  public static IsMac(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^([0-9A-F]{2}-){5}[0-9A-F]{2}$/.test(value) || /^[0-9A-F]{12}$/.test(value);
  }

  /**
   * 是否IP地址
   * @param value ip地址
   * @returns 是否有效
   */
  public static IsIpAddress(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^(\d(25[0-5]|2[0-4][0-9]|1?[0-9]?[0-9])\d\.){3}\d(25[0-5]|2[0-4][0-9]|1?[0-9]?[0-9])\d$/.test(value);
  }

  /**
   * 是否中文
   * @param value 中文
   * @returns 是否有效
   */
  public static IsChinese(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^[\u4e00-\u9fa5]+$/.test(value);
  }

  /**
   * 是否包含中文
   * @param value 中文
   * @returns 是否包含
   */
  public static IsContainsChinese(value: string): boolean {
    if (!value) {
      return false;
    }
    return /[\u4e00-\u9fa5]+/.test(value);
  }

  /**
   * 是否包含数字
   * @param value 数字
   * @returns 是否包含
   */
  public static IsContainsNumber(value: string): boolean {
    if (!value) {
      return false;
    }
    return /[0-9]+/.test(value);
  }

  /**
   * 是否正常字符，字母、数字、下划线的组合
   * @param value 字符串
   * @returns 是否有效
   */
  public static IsNormalChar(value: string): boolean {
    if (!value) {
      return false;
    }
    return /[\w\d_]+/.test(value);
  }

  /**
   * 是否重复
   * @param value 值
   * @returns 是否重复
   */
  public static IsRepeat(value: string): boolean {
    if (!value) {
      return false;
    }
    const array = value.split('');
    return array.some(c => array.filter(t => t === c).length > 1);
  }

  /**
   * 是否邮政编码
   * @param value 邮政编码
   * @returns 是否有效
   */
  public static IsPostalCode(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^[1-9]\d{5}$/.test(value);
  }

  /**
   * 是否中国电话
   * @param value 电话
   * @returns 是否有效
   */
  public static IsTel(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^\d{3,4}-?\d{6,8}$/.test(value);
  }

  /**
   * 是否合法QQ号码
   * @param value QQ号码
   * @returns 是否有效
   */
  public static IsQQ(value: string): boolean {
    if (!value) {
      return false;
    }
    return /^[1-9][0-9]{4,9}$/.test(value);
  }


}
