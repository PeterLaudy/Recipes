import { GerechtDB, Gerecht } from "./gerecht.model";
import { HoeveelheidDB, Hoeveelheid } from "./hoeveelheid.model";

export interface IReceptDB {
    gerecht: GerechtDB;
    hoeveelheden: HoeveelheidDB[];
}

export class ReceptDB implements IReceptDB {
    constructor(recept: Recept) {
        this.gerecht = new GerechtDB(recept.gerecht);
        this.hoeveelheden = [];
        recept.hoeveelheden.forEach(h =>{
            this.hoeveelheden.push(new HoeveelheidDB(h));
        });
    }

    public gerecht: GerechtDB;
    public hoeveelheden: HoeveelheidDB[];
}

export class Recept {
    constructor(record: IReceptDB) {
        if (record) {
            this.gerecht = new Gerecht(record.gerecht);
            this.hoeveelheden = [];
            record.hoeveelheden.forEach(h =>{
                this.hoeveelheden.push(new Hoeveelheid(h));
            });
        } else {
            this.gerecht = new Gerecht(null);
            this.hoeveelheden = [];
        }
    }

    public gerecht: Gerecht;
    public hoeveelheden: Hoeveelheid[];
}