import {bootstrap} from "aurelia-bootstrapper";
import {ComponentTester, StageComponent} from "aurelia-testing";

describe("Stage App Component", () => {
  let component: ComponentTester<unknown>;

  beforeEach(() => {
    component = StageComponent
      .withResources("app")
      .inView("<app></app>");
  });

  afterEach(() => component.dispose());

  it("should render message", done => {
    component.create(bootstrap).then(() => {
      const view = component.element;
      expect(view.textContent.trim()).toBe("Hello World!");
      done();
    }).catch(e => {
      fail(e);
      done();
    });
  });
});
