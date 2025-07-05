import { RouteDefinition } from "@andrewmclachlan/mooapp";
import App from "App";
import * as Pages from "./pages";
import { MonthlyBalances } from "pages/reports/MonthlyBalances";

export const routes: RouteDefinition = {
    layout: {
        path: "/", element: <App />, children: {
            home: { path: "/", element: <Pages.Dashboard /> },
            accounts: { path: "/accounts", element: <Pages.Accounts /> },
            accountsCreate: { path: "/accounts/create", element: <Pages.CreateAccount /> },
            account: {
                path: "/accounts/:id", element: <Pages.Account />, children: {
                    manage: { path: "manage", element: <Pages.ManageAccount /> },
                    transactions: { path: "transactions", element: <Pages.Transactions /> },
                    virtualCreate: { path: "manage/virtual/create", element: <Pages.CreateVirtualAccount /> },
                    virtualManage: { path: "manage/virtual/:virtualId", element: <Pages.ManageVirtualAccount /> },
                    rules: { path: "rules", element: <Pages.Rules /> },
                    reports: {
                        path: "reports", element: <Pages.Reports />, children: {
                            inout: { path: "in-out", element: <Pages.InOutPage /> },
                            breakdown: { path: "breakdown/:tagId?", element: <Pages.BreakdownPage /> },
                            byTag: { path: "by-tag", element: <Pages.ByTag /> },
                            tagTrend: { path: "tag-trend/:tagId?", element: <Pages.TagTrend /> },
                            allTagAverage: { path: "all-tag-average", element: <Pages.AllTagAverage /> },
                            MonthlyBalances: {path: "monthly-balances", element: <MonthlyBalances /> },
                        }
                    },
                    virtual: {
                        path: "virtual/:virtualId", element: <Pages.VirtualAccount />, children: {
                            transactions: { path: "transactions", element: <Pages.Transactions /> },
                            manage: { path: "manage", element: <Pages.ManageVirtualAccount /> },
                        }
                    },
                }
            },
            billAccountSummaries: { path: "/bills", element: <Pages.BillAccountSummaries /> },
            billAccountsByType: { path: "/bills/:id", element: <Pages.BillAccounts /> },
            bills: { path: "/bills/accounts/:id", element: <Pages.Bills /> },
            stockCreate: { path: "/shares/create", element: <Pages.CreateStockHolding /> },
            stock: {
                path: "/shares/:id", element: <Pages.StockHolding />, children: {
                    manage: { path: "manage", element: <Pages.ManageStockHolding /> },
                    transactions: { path: "transactions", element: <Pages.StockTransactions /> },
                    transactionsAdd: { path: "transactions/add", element: <Pages.AddStockTransaction /> },
                    reports: {
                        path: "reports", element: <Pages.StockReports />, children: {
                            value: { path: "value", element: <Pages.StockValueReport /> },
                        }
                    },
                },
            },
            assetCreate: { path: "/assets/create", element: <Pages.CreateAsset /> },
            asset: {
                path: "/assets/:id", element: <Pages.Asset />, children: {
                    manage: { path: "manage", element: <Pages.ManageAsset /> },
                }
            },
            budget: { path: "/budget", element: <Pages.Budget /> },
            budgetReport: { path: "/budget/report/:year?/:month?", element: <Pages.BudgetReport /> },
            settings: {
                path: "/settings", element: <Pages.Settings />, children: {
                    families: { path: "families", element: <Pages.Families /> },
                    familiesAdd: { path: "families/add", element: <Pages.CreateFamily /> },
                    family: { path: "families/:id", element: <Pages.ManageFamily /> },
                    institutions: { path: "institutions", element: <Pages.Institutions /> },
                    institutionsAdd: { path: "institutions/add", element: <Pages.CreateInstitution /> },
                    institution: { path: "institutions/:id", element: <Pages.ManageInstitution /> },
                }
            },
            tags: { path: "/tags", element: <Pages.TransactionTags /> },
            tagsVisualiser: { path: "/tags/visualiser", element: <Pages.Visualiser /> },
            groups: { path: "/groups", element: <Pages.ManageGroups /> },
            groupsManage: { path: "/groups/:id/manage", element: <Pages.ManageGroup /> },
            groupsCreate: { path: "/groups/create", element: <Pages.CreateGroup /> },
            profile: { path: "/profile", element: <Pages.Profile /> },
            fallback: { path: "*", element: <Pages.Error404 /> },
        }
    }
};
