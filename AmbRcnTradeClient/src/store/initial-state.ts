import { Approval, Currency } from "constants/app-constants";
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
      weightKg: undefined!,
      avgBagWeightKg: undefined!,
      id: undefined!,
      name: undefined!,
      companyId: undefined!,
      analyses: [],
      supplierId: undefined!,
      stockReferences: [],
      origin: undefined!,
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
      fiche: undefined!,  
      stuffingRecords: []
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
      deliveryDate: undefined!,
      value: undefined!,
      valueUsd: undefined!
    },
    list: [],
    nonCommittedStocksList: []
  },
  stockManagement: {
    stuffContainer: [],
    availableContainers: []
  },
  container: { current: undefined!, list: [] },
  vessel: {
    current: {
      id: undefined!,
      companyId: undefined!,
      name: undefined!,
      containersOnBoard: undefined!,
      billLadingIds: [],
      billLadings: [],
      forwardingAgentId: undefined!,
      shippingCompanyId: undefined!,
      voyageNumber: undefined!,
      vesselName: undefined!,
      eta: undefined!,
      notes: undefined!,
      bookingNumber: undefined!,
      serviceContract: undefined!
    },
    list: [],
    notLoadedContainers: []
  },
  billLading: {
    current: {
      notifyParty1Id: undefined!,
      notifyParty2Id: undefined!,
      consigneeId: undefined!,
      blBodyText: undefined!,
      shipperId: undefined!,
      freightPrepaid: true!,
      containersOnBoard: undefined!,
      blDate: undefined!,
      blNumber: undefined!,
      containerIds: [],
      containers: [],
      companyId: undefined!,
      id: undefined!,
      name: undefined!,
      vesselId: undefined!,
      ownReferences: undefined!,
      shipperReference: undefined!,
      consigneeReference: undefined!,
      portOfDestinationId: undefined!,
      portOfDestinationName: undefined!,
      portOfLoadingId: undefined!,
      destinationAgentId: undefined!,
      shippingMarks: undefined!,
      forwarderReference: undefined!,
      productDescription: undefined!,
      preCargoDescription: undefined!,
      nettWeightKg: undefined!,
      nettWeightkgText: undefined!,
      grossWeightKg: undefined!,
      grossWeightKgText: undefined!,
      numberBags: undefined!,
      numberPackagesText: undefined!,
      oceanFreight: undefined,
      freightDestinationCharge: undefined!,
      freightDestinationChargePaidBy: undefined!,
      freightOriginCharges: undefined!,
      freightOriginChargesPaidBy: undefined!,
      teu: undefined!,
      vgmWeightKgText: undefined!,
      oceanFreightPaidBy: undefined!
    },
    list: []
  },
  payment: {
    current: {
      paymentDate: undefined!,
      beneficiaryId: undefined!,
      value: undefined!,
      currency: Currency.CFA,
      exchangeRate: undefined!,
      supplierId: undefined!,
      notes: undefined!,
      id: undefined!,
      name: undefined!,
      companyId: undefined!,
      paymentNo: undefined!
    },
    paymentDto: {
      paymentList: [],
      purchaseList: [],
      purchaseValue: undefined!,
      purchaseValueUsd: undefined!,
      paymentValue: undefined!,
      paymentValueUsd: undefined!
    },
    list: []
  }
};
