import "./PageHeader.scss";

import React from "react";
import { MenuItem } from "../models/MenuItem";
import { Link } from "react-router-dom";

export const PageHeader: React.FC<PageHeaderProps> = (props) => {
    return (
        <section className="page-header">
            <header>
                <h1>{props.title}</h1>
                <nav className="control-panel">
                    <ul>
                        {renderMenu(props.menuItems)}
                    </ul>
                </nav>
            </header>
            {props.children}
        </section>
    )
}

PageHeader.displayName = "PageHeader";

const renderMenu = (menuItems: MenuItem[]) => {

    const items = [];

    let keysuffix = 0;

    for (const menuItem of menuItems) {

        if (menuItem.route) {
            items.push(<li key={"route" + keysuffix.toString()}><Link to={menuItem.route} onClick={() => menuItem.onClick && menuItem.onClick()}>{menuItem.text}</Link></li>);
        }
        else if (menuItem.onClick) {
            items.push(<li key={"click" + keysuffix.toString()}><button onClick={() => menuItem.onClick()}>{menuItem.text}</button></li>);
        }

        keysuffix++;
    }

    return items;

}

interface PageHeaderProps {
    title: string;
    menuItems: MenuItem[];
}