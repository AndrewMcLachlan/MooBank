import React from "react";
import { MenuItem } from "../models/MenuItem";
import { Link } from "react-router-dom";
import { Breadcrumb, Container } from "react-bootstrap";
import { LinkContainer } from "react-router-bootstrap";

export type PageHeaderComponent = React.FC<PageHeaderProps>;

export const PageHeader: React.FC<PageHeaderProps> = (props) => {

    return (
        <header>
            <header className="page-header">
                <Container>
                    <h2>{props.title}</h2>

                    {props.children}
                </Container>
            </header>
            <Container className="secondary-nav" hidden={props.menuItems!.length === 0 && props.hidebreadcrumb}>
                <section className="nav">
                    <Breadcrumb hidden={props.hidebreadcrumb}>
                        <LinkContainer to="/">
                            <Breadcrumb.Item>Home</Breadcrumb.Item>
                        </LinkContainer>
                        {props.breadcrumbs.map(([name, to]) =>
                            <LinkContainer key={to} to={to}>
                                <Breadcrumb.Item>{name}</Breadcrumb.Item>
                            </LinkContainer>
                        )}
                    </Breadcrumb>
                    <nav className="control-panel">
                        <ul>
                            {renderMenu(props.menuItems)}
                        </ul>
                    </nav>
                </section>
            </Container>
        </header>
    );
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

PageHeader.defaultProps = {
    menuItems: [],
    breadcrumbs: [],
    hidebreadcrumb: false,
}

interface PageHeaderProps {
    title: string;
    menuItems?: MenuItem[];
    goBack?: string;
    breadcrumbs?: [string, string][];
    hidebreadcrumb?: boolean;
}