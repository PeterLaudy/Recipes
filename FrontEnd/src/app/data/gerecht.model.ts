import { Categorie } from "./categorie.model";

export interface IGerechtDB {
    gerechtID: number;
    naam: string;
    omschrijving: string;
    minuten: number;
}

export class GerechtDB implements IGerechtDB {
    constructor(gerecht: Gerecht) {
        this.gerechtID = gerecht.index;
        this.naam = gerecht.name;
        this.omschrijving = gerecht.description;
        this.minuten = gerecht.minutes;
    }

    public gerechtID: number;
    public naam: string;
    public omschrijving: string;
    public minuten: number;
}

export class Gerecht {
    constructor(record: IGerechtDB) {
        if (record) {
            this.index = record.gerechtID;
            this.name = record.naam;
            this.description = record.omschrijving;
            this.minutes = record.minuten;
        } else {
            this.index = 0;
            this.name = "";
            this.description = "";
            this.minutes = 0;
        }
    }

    public index: number;
    public name: string;
    public description: string;
    public minutes: number;
}

export class GerechtSummary {
    public index: number;
    public name: string;
    public categorieen: Categorie[];
}