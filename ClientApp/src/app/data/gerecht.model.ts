export interface IGerechtDB {
    gerechtID: number;
    naam: string;
    omschrijving: string;
    minuten: number;
    categorieID: number;
}

export class GerechtDB implements IGerechtDB {
    constructor(gerecht: Gerecht) {
        this.gerechtID = gerecht.index;
        this.naam = gerecht.name;
        this.omschrijving = gerecht.description;
        this.minuten = gerecht.minutes;
        this.categorieID = gerecht.categorieID;
    }

    public gerechtID: number;
    public naam: string;
    public omschrijving: string;
    public minuten: number;
    public categorieID: number;
}

export class Gerecht {
    constructor(record: IGerechtDB) {
        if (record) {
            this.index = record.gerechtID;
            this.name = record.naam;
            this.description = record.omschrijving;
            this.minutes = record.minuten;
            this.categorieID = record.categorieID;
        } else {
            this.index = 0;
            this.name = "";
            this.description = "";
            this.minutes = 0;
            this.categorieID = 1;
        }
    }

    public index: number;
    public name: string;
    public description: string;
    public minutes: number;
    public categorieID: number;
}

export class GerechtSummary {
    public index: number;
    public name: string;
}

export class GerechtSummaryList {
    public categorie: string;
    public list: GerechtSummary[];
}