import type { AccountType, LogicalAccount } from "api/types.gen";

export const AccountTypes: AccountType[] = ["NotSet", "Transaction", "Savings", "Credit", "Mortgage", "Superannuation", "Investment", "Loan", "Broker"];

export const emptyAccount: LogicalAccount = {
    id: "",
    name: "",
    currentBalance: 0,
    remainingBalance: 0,
    balanceDate: new Date().toISOString(),
    accountType: "NotSet",
    controller: "Manual",
    groupId: "",
    currentBalanceLocalCurrency: 0,
    currency: "",
    shareWithFamily: false,
    includeInBudget: false,
    institutionAccounts: [],
    isPrimary: false,
    virtualInstruments: [],
};
