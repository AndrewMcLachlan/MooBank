import React from "react";
import { MenuItem } from "../models/MenuItem";
import { Link } from "react-router-dom";
import { Breadcrumb, Button, Container } from "react-bootstrap";
import { LinkContainer } from "react-router-bootstrap";

export type PageHeaderComponent = React.FC<React.PropsWithChildren<PageHeaderProps>>;

export const PageHeader: PageHeaderComponent = (props) => {

    return (
        <>
            <header className="page-header">
                <Container>
                    <h2>{props.title}</h2>
                    {props.children}
                    <nav className="control-panel">
                        <ul>
                            {renderMenu(props.menuItems!)}
                        </ul>
                    </nav>
                </Container>
            </header>
            <header className="breadcrumb-container">
                <Container hidden={props.menuItems!.length === 0 && props.hidebreadcrumb}>
                    <Breadcrumb hidden={props.hidebreadcrumb}>
                        <LinkContainer to="/">
                            <Breadcrumb.Item>Home</Breadcrumb.Item>
                        </LinkContainer>
                        {props.breadcrumbs?.map(([name, to]) =>
                            <LinkContainer key={to} to={to}>
                                <Breadcrumb.Item>{name}</Breadcrumb.Item>
                            </LinkContainer>
                        )}
                    </Breadcrumb>
                </Container>
            </header>
        </>
    );
}

PageHeader.displayName = "PageHeader";

const renderMenu = (menuItems: MenuItem[]) => {

    const items: React.ReactNode[] = [];

    let keysuffix = 0;

    for (const menuItem of menuItems) {

        if (menuItem.route) {
            items.push(<li key={"route" + keysuffix.toString()}><Link to={menuItem.route} onClick={() => menuItem.onClick && menuItem.onClick()}>{menuItem.text}</Link></li>);
        }
        else if (menuItem.onClick) {
            items.push(<li key={"click" + keysuffix.toString()}><Button variant="link" onClick={() => menuItem.onClick()}>{menuItem.text}</Button></li>);
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