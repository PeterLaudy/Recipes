export interface IRegisterData {
    Token: string;
    UserName: string;
}

export class RegisterData implements IRegisterData {
    constructor(token, username: string) {
        this.Token = token;
        this.UserName = username;
    }

    public Token: string;
    public UserName: string;
}