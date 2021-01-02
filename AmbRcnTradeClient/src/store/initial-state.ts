import { Approval } from "constants/app-constants";
import { IState } from "./state";

export const initialState: IState = {
  currentCompanyId: "companies/1-A",
  currentCompanyName: undefined!,
  culture: "en",
  loggedIn: false,
  user: {
    email: undefined!,
    id: undefined!,
    name: undefined!,
    firstName: undefined!,
    lastName: undefined!,
    role: undefined!,
    companies: []
  },
  userCompanies: [],
  userCustomers: [],
  userFilteredCustomers: [],
  serverMessages: {
    errorMessage: "",
    message: ""
  },
  admin: {
    company: {
      current: {
        users: [],
        userDetails: [],
        contact: undefined!,
        address: undefined!,
        taxId: undefined!,
        activeSubscription: undefined!,
        created: undefined!,
        id: undefined!,
        name: undefined!,
        accessCode: undefined!,
        demoMode: undefined!
      },
      list: []
    },
    user: {
      current: {
        id: undefined!,
        name: undefined!,
        approved: undefined!,
        role: undefined!,
        password: undefined!,
        email: undefined!,
        companyName: undefined!,
        firstName: undefined!,
        lastName: undefined!,
        clientCompaniesList: [],
        companyDetails: {
          companyName: undefined!,
          address: undefined!,
          notes: undefined!,
          taxId: undefined!,
          tel: undefined!
        }
      },
      list: []
    },
    roleNames: []
  },
  customer: {
    current: undefined!,
    list: [],
    usersList: []
  },

  port: {
    current: undefined!,
    list: []
  },
  attachmentRoutes: [],
  inspection: {
    current: {
      inspectionDate: undefined!,
      inspector: undefined!,
      lotNo: undefined!,
      location: undefined!,
      truckPlate: undefined!,
      bags: undefined!,
      id: undefined!,
      name: undefined!,
      companyId: undefined!,
      analyses: [],
      supplierId: undefined!,
      stockReferences: [],
      analysisResult: {
        count: 0,
        moisture: 0,
        kor: 0,
        approved: Approval.Rejected,
        soundPct: undefined!,
        spottedPct: undefined!,
        rejectsPct: undefined!,
        inspectionId: undefined!
      }
    },
    list: [],
    movedToStockId: undefined!
  },
  stock: {
    current: {
      id: undefined!,
      name: undefined!,
      companyId: undefined!,
      locationId: undefined!,
      stockInDate: undefined!,
      stockOutDate: undefined!,
      lotNo: undefined!,
      bags: undefined!,
      weightKg: undefined!,
      inspectionId: undefined!,
      isStockIn: undefined!,
      origin: undefined!,
      supplierId: undefined!,
      analysisResult: undefined!,
      containerId: undefined!
    },
    list: [],
    stockBalanceList: []
  },
  purchase: {
    current: {
      id: undefined!,
      name: undefined!,
      companyId: undefined!,
      purchaseDate: undefined!,
      purchaseNumber: undefined!,
      supplierId: undefined!,
      quantityMt: undefined!,
      purchaseDetails: [],
      deliveryDate: undefined!
    },
    list: [],
    nonCommittedStocksList: []
  },
  stockManagement: {
    stuffContainer: []
  },
  container: { current: undefined!, list: [] }
};
