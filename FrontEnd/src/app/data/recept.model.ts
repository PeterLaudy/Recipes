import { Categorie } from "./categorie.model";
import { GerechtDB, Gerecht } from "./gerecht.model";
import { HoeveelheidDB, Hoeveelheid } from "./hoeveelheid.model";

export interface IReceptDB {
    gerecht: GerechtDB;
    categorieen: Categorie[];
    hoeveelheden: HoeveelheidDB[];
}

export class ReceptDB implements IReceptDB {
    constructor(recept: Recept) {
        this.gerecht = new GerechtDB(recept.gerecht);
        this.categorieen = recept.categorieen;
        this.hoeveelheden = [];
        recept.hoeveelheden.forEach(h =>{
            this.hoeveelheden.push(new HoeveelheidDB(h));
        });
    }

    public gerecht: GerechtDB;
    public categorieen: Categorie[];
    public hoeveelheden: HoeveelheidDB[];
}

export class Recept {
    constructor(record: IReceptDB) {
        if (record) {
            this.gerecht = new Gerecht(record.gerecht);
            this.categorieen = record.categorieen;
            this.hoeveelheden = [];
            record.hoeveelheden.forEach(h =>{
                this.hoeveelheden.push(new Hoeveelheid(h));
            });
        } else {
            this.gerecht = new Gerecht(null);
            this.categorieen = [];
            this.hoeveelheden = [];
        }
    }

    public gerecht: Gerecht;
    public categorieen: Categorie[];
    public hoeveelheden: Hoeveelheid[];
}