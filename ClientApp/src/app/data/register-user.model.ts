export interface IRegisterUserData {
    FirstName: string;
    LastName: string;
    UserName: string;
    EmailAddress: string;
}

export class RegisterUserData implements IRegisterUserData {
    constructor(firstname, lastname, username, emailAddress: string) {
        this.FirstName = firstname;
        this.LastName = lastname;
        this.UserName = username;
        this.EmailAddress = emailAddress;
    }

    public FirstName: string;
    public LastName: string;
    public UserName: string;
    public EmailAddress: string;
}