export interface IRegisterData {
    Token: string;
    UserName: string;
    EmailAddress: string;
    Password: string;
    ConfirmPassword: string;
}

export class RegisterData implements IRegisterData {
    constructor(token, username, emailAddress, password, confirmPassword: string) {
        this.Token = token;
        this.UserName = username;
        this.EmailAddress = emailAddress;
        this.Password = password;
        this.ConfirmPassword = confirmPassword;
    }

    public Token: string;
    public UserName: string;
    public EmailAddress: string;
    public Password: string;
    public ConfirmPassword: string;
}