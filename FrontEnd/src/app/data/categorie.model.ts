export interface ICategorieDB {
    categorieID: number;
    naam: string;
}

export class CategorieDB implements ICategorieDB {
    constructor(categorie: Categorie) {
        this.categorieID = categorie.index;
        this.naam = categorie.name;
    }

    categorieID: number;
    naam: string;
}

export class Categorie {
    constructor(record: ICategorieDB) {
        if (record) {
            this.index = record.categorieID;
            this.name = record.naam;
        } else {
            this.index = 0;
            this.name = "";
        }
    }

    index: number;
    name: string;
}