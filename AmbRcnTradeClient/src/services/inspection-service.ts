import { HttpClient } from "aurelia-fetch-client";
import { autoinject } from "aurelia-framework";
import { Router } from "aurelia-router";
import { Store } from "aurelia-store";
import { Fn } from "core/types";
import _ from "lodash";
import { QueryId } from "models/QueryId";
import { IState } from "store/state";
import { Approval } from "./../constants/app-constants";
import { fixAspNetCoreDate } from "./../core/helpers";
import { IAnalysis } from "./../interfaces/inspections/IAnalysis";
import { IInspection } from "./../interfaces/inspections/IInspection";
import { IInspectionListItem } from "./../interfaces/inspections/IInspectionListItem";
import { FetchService } from "./fetch-service";

@autoinject
export class InspectionService extends FetchService {
  constructor(
    http: HttpClient,
    store: Store<IState>,
    router: Router
  ) {
    super("api/inspection", http, store, router);

    store.registerAction("inspectionEditAction", inspectionEditAction);
    store.registerAction("inspectionListAction", inspectionListAction);
  }

  public async load(id: string) {
    return super.get(id, "load", inspectionEditAction);
  }

  public async save(model: IInspection) {
    model.inspectionDate = fixAspNetCoreDate(model.inspectionDate, false);

    return super.post(model, "save", inspectionEditAction);
  }

  public async createInspection() {
    return super.get([], "create", inspectionEditAction);
  }

  public async loadList(approval: Approval) {
    return super.getMany<IInspectionListItem[]>([super.currentCompanyIdQuery(), new QueryId("approval", approval)], "loadList", inspectionListAction);
  }

  public canAddInspectionToStock(inspection: IInspection): boolean {
    const bagsAlreadyAllocated = this.bagsAlreadyAllocated(inspection);
    if (!bagsAlreadyAllocated) {
      return true;
    }
    return bagsAlreadyAllocated < inspection.bags;
  }

  public bagsAlreadyAllocated(inspection: IInspection):number{
    return inspection.stockReferences.reduce((a, b) => a += b.bags, 0);
  }

  public getAnalysisResult(list: IAnalysis[], approved: Approval): IAnalysis {
    const bags = list.reduce((a, b) => a += b.bags, 0);

    if (bags === 0) {
      return {} as IAnalysis;
    }

    const array: IAnalysis[] = [];

    list.forEach(item => {
      item.kor = this.calcKor(item);

      const { soundPct, spottedPct, rejectsPct } = this.calcPercentages(item);

      item.soundPct = soundPct;
      item.spottedPct = spottedPct;
      item.rejectsPct = rejectsPct;

      array.push(
        {
          bags: this.calcAverageResult(item, c => c.bags),
          count: this.calcAverageResult(item, c => c.count),
          moisture: this.calcAverageResult(item, c => c.moisture),
          kor: this.calcAverageResult(item, c => c.kor),
          soundPct: this.calcAverageResult(item, c => c.soundPct),
          rejectsPct: this.calcAverageResult(item, c => c.rejectsPct),
          spottedPct: this.calcAverageResult(item, c => c.spottedPct),
          soundGm: 0,
          rejectsGm: 0,
          spottedGm: 0,
          approved
        });
    });

    const result: IAnalysis = {
      bags: this.sumAnalysisResult(array, c => c.bags) / bags,
      count: this.sumAnalysisResult(array, c => c.count) / bags,
      moisture: this.sumAnalysisResult(array, c => c.moisture) / bags,
      kor: this.sumAnalysisResult(array, c => c.kor) / bags,
      soundPct: this.sumAnalysisResult(array, c => c.soundPct) / bags,
      rejectsPct: this.sumAnalysisResult(array, c => c.rejectsPct) / bags,
      spottedPct: this.sumAnalysisResult(array, c => c.spottedPct) / bags,
      soundGm: 0,
      rejectsGm: 0,
      spottedGm: 0,
      approved
    };

    return result;
  }

  private calcAverageResult(analysis: IAnalysis, field: Fn<IAnalysis, number>): number {
    return analysis.bags * (field(analysis) || 0);
  }

  private sumAnalysisResult(list: IAnalysis[], field: Fn<IAnalysis, number>) {
    return list.reduce((a, b) => a += field(b), 0);
  }

  private calcKor(analysis: IAnalysis): number {
    if (analysis.soundGm && analysis.spottedGm) {
      return (((analysis.spottedGm / 2) + analysis.soundGm) * 80 * 2.20462 / 1000) || 0;
    }
    return 0;
  }

  private calcPercentages(analysis: IAnalysis) {
    const total = (analysis?.soundGm || 0) + (analysis?.spottedGm || 0) + (analysis?.rejectsGm || 0);
    if (total === 0) {
      return {
        soundPct: 0,
        spottedPct: 0,
        rejectsPct: 0
      };
    }

    return {
      soundPct: (analysis?.soundGm || 0) / total,
      spottedPct: (analysis?.spottedGm || 0) / total,
      rejectsPct: (analysis?.rejectsGm || 0) / total
    };
  }
}

export function inspectionEditAction(state: IState, inspection: IInspection) {
  inspection.inspectionDate = fixAspNetCoreDate(inspection.inspectionDate, false);
  inspection.stockReferences.forEach(c => {
    c.date = fixAspNetCoreDate(c.date, false);
  });

  const newState = _.cloneDeep(state);
  newState.inspection.current = inspection;
  return newState;
}

export function inspectionListAction(state: IState, list: IInspectionListItem[]) {
  const newState = _.cloneDeep(state);
  newState.inspection.list = list;
  return newState;
}

