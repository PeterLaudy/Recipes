export interface IEenheidDB {
    eenheidID: number;
    naam: string;
}

export class EenheidDB implements IEenheidDB {
    constructor(eenheid: Eenheid) {
        this.eenheidID = eenheid.index;
        this.naam = eenheid.name.toLowerCase();
    }

    eenheidID: number;
    naam: string;
}

export class Eenheid {
    constructor(record: IEenheidDB) {
        if (record) {
            this.index = record.eenheidID;
            this.name = record.naam.toLowerCase();
        } else {
            this.index = 0;
            this.name = "";
        }
    }

    index: number;
    name: string;
}