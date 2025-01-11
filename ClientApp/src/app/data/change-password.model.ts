export interface IChangePasswordData {
    Token: string;
    UserName: string;
    Password: string;
    ConfirmPassword: string;
}

export class ChangePasswordData implements IChangePasswordData {
    constructor(token, username, password, confirmpassword: string) {
        this.Token = token;
        this.UserName = username;
        this.Password = password;
        this.ConfirmPassword = confirmpassword;
    }

    public Token: string;
    public UserName: string;
    public Password: string;
    public ConfirmPassword: string;
}