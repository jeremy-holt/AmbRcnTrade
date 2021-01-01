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
import { IAnalysis, IAnalysisResult } from "./../interfaces/inspections/IAnalysis";
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

  public bagsAlreadyAllocated(inspection: IInspection): number {
    return inspection.stockReferences.reduce((a, b) => a += b.bags, 0);
  }

  public wouldExceedInspectionBags(inspection: IInspection, bagsToAdd: number):boolean{
    return this.bagsAlreadyAllocated(inspection) + bagsToAdd > inspection.bags;
  }

  public getAnalysisResult(analyses: IAnalysis[], approved: Approval): IAnalysisResult {
    const arrayAnalysisResult: IAnalysisResult[] = [];

    analyses.forEach(analysis => {
      analysis.kor = this.calcKor(analysis);

      const { soundPct, spottedPct, rejectsPct } = this.calcPercentages(analysis);

      arrayAnalysisResult.push(
        {
          count: analysis.count,
          moisture: analysis.moisture,
          kor: analysis.kor,
          soundPct: soundPct,
          rejectsPct: rejectsPct,
          spottedPct: spottedPct,
          approved,
          inspectionId:undefined!
        });
    });

    const result: IAnalysisResult = {
      count: this.sumAnalysisResult(arrayAnalysisResult, c => c.count),
      moisture: this.sumAnalysisResult(arrayAnalysisResult, c => c.moisture),
      kor: this.sumAnalysisResult(arrayAnalysisResult, c => c.kor),
      soundPct: this.sumAnalysisResult(arrayAnalysisResult, c => c.soundPct),
      rejectsPct: this.sumAnalysisResult(arrayAnalysisResult, c => c.rejectsPct),
      spottedPct: this.sumAnalysisResult(arrayAnalysisResult, c => c.spottedPct),
      approved,
      inspectionId:undefined!
    };

    return result;
  }

  private sumAnalysisResult(array: IAnalysisResult[], field: Fn<IAnalysisResult, number>): number {
    if (array.length === 0) {
      return 0;
    }
    return array.reduce((a, b) => a += field(b), 0) / array.length;
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

