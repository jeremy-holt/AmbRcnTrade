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
    // model.analysisResult = this.zeroAnalysisResult(model.analysisResult);

    model.inspectionDate = fixAspNetCoreDate(model.inspectionDate, false);

    return super.post(model, "save", inspectionEditAction);
  }

  public async createInspection() {
    return super.get([], "create", inspectionEditAction);
  }

  public async loadList(approval: Approval) {
    return super.getMany<IInspectionListItem[]>([super.currentCompanyIdQuery(), new QueryId("approval", approval)], "loadList", inspectionListAction);
  }

  public getAnalysisResult(list: IAnalysis[], approved: Approval): IAnalysis {
    const bags = list.reduce((a, b) => a += b.bags, 0);

    if (bags === 0) {
      return {} as IAnalysis;
    }

    const array: IAnalysis[] = [];

    list.forEach(item => {
      item.kor = this.calcKor(item);

      const { sound, spotted, rejects } = this.calcPercentages(item);
      
      item.sound = sound;
      item.spotted = spotted;
      item.rejects = rejects;

      array.push(
        {
          bags: this.calcAverageResult(item, c => c.bags),
          count: this.calcAverageResult(item, c => c.count),
          moisture: this.calcAverageResult(item, c => c.moisture),
          kor: this.calcAverageResult(item, c => c.kor),
          sound: this.calcAverageResult(item, c => c.sound),
          rejects: this.calcAverageResult(item, c => c.rejects),
          spotted: this.calcAverageResult(item, c => c.spotted),
          approved
        });
    });

    const result: IAnalysis = {
      bags: this.sumAnalysisResult(array, c => c.bags) / bags,
      count: this.sumAnalysisResult(array, c => c.count) / bags,
      moisture: this.sumAnalysisResult(array, c => c.moisture) / bags,
      kor: this.sumAnalysisResult(array, c => c.kor) / bags,
      sound: this.sumAnalysisResult(array, c => c.sound) / bags,
      rejects: this.sumAnalysisResult(array, c => c.rejects) / bags,
      spotted: this.sumAnalysisResult(array, c => c.spotted) / bags,
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
    if (analysis.sound && analysis.spotted) {
      return (((analysis.spotted / 2) + analysis.sound) * 80 * 2.20462 / 1000) || 0;
    }
    return 0;
  }

  private calcPercentages(analysis: IAnalysis) {
    const total = (analysis?.sound || 0) + (analysis?.spotted || 0) + (analysis?.rejects || 0);
    if (total === 0) {
      return {
        sound: 0,
        spotted: 0,
        rejects: 0
      };
    }

    return {
      sound: (analysis?.sound || 0) / total,
      spotted: (analysis?.spotted || 0) / total,
      rejects: (analysis?.rejects || 0) / total
    };
  }

  private calc(analysis: IAnalysis | IAnalysis[], field: Fn<IAnalysis, number>): number {
    if (Array.isArray(analysis)) {
      return analysis.reduce((a, b) => a += (field(b) || 0), 0);
    }
    return analysis.bags * (field(analysis) || 0);
  }
}

export function inspectionEditAction(state: IState, inspection: IInspection) {
  inspection.inspectionDate = fixAspNetCoreDate(inspection.inspectionDate, false);

  const newState = _.cloneDeep(state);
  newState.inspection.current = inspection;
  return newState;
}

export function inspectionListAction(state: IState, list: IInspectionListItem[]) {
  const newState = _.cloneDeep(state);
  newState.inspection.list = list;
  return newState;
}

