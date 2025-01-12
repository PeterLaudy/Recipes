import { ICategorieDB, CategorieDB, Categorie } from "./categorie.model";

export interface IGerechtDB {
    gerechtID: number;
    naam: string;
    omschrijving: string;
    minuten: number;
    categorie: ICategorieDB;
}

export class GerechtDB implements IGerechtDB {
    constructor(gerecht: Gerecht) {
        this.gerechtID = gerecht.index;
        this.naam = gerecht.name;
        this.omschrijving = gerecht.description;
        this.minuten = gerecht.minutes;
        this.categorie = new CategorieDB(gerecht.categorie);
    }

    public gerechtID: number;
    public naam: string;
    public omschrijving: string;
    public minuten: number;
    public categorie: CategorieDB;
}

export class Gerecht {
    constructor(record: IGerechtDB) {
        if (record) {
            this.index = record.gerechtID;
            this.name = record.naam;
            this.description = record.omschrijving;
            this.minutes = record.minuten;
            this.categorie = new Categorie(record.categorie);
        } else {
            this.index = 0;
            this.name = "";
            this.description = "";
            this.minutes = 0;
            this.categorie = new Categorie(null);
        }
    }

    public index: number;
    public name: string;
    public description: string;
    public minutes: number;
    public categorie: Categorie;
}

export class GerechtSummary {
    public index: number;
    public name: string;
}

export class GerechtSummaryList {
    public categorie: Categorie;
    public list: GerechtSummary[];
}