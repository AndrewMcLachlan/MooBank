"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
var react_1 = require("react");
var react_router_1 = require("react-router");
var Layout_1 = require("./components/Layout");
var Home_1 = require("./components/Home");
var Counter_1 = require("./components/Counter");
var FetchData_1 = require("./components/FetchData");
exports.default = (function () { return (react_1.default.createElement(Layout_1.default, null,
    react_1.default.createElement(react_router_1.Route, { exact: true, path: '/', component: Home_1.default }),
    react_1.default.createElement(react_router_1.Route, { path: '/counter', component: Counter_1.default }),
    react_1.default.createElement(react_router_1.Route, { path: '/fetch-data/:startDateIndex?', component: FetchData_1.default }))); });
//# sourceMappingURL=App.js.map