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
  userFilteredCustomers:[],
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
  attachmentRoutes:[]
};
