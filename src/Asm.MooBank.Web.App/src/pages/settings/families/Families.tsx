import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Table } from "react-bootstrap";
import { Link } from "react-router-dom";
import { useFamilies } from "services";
import { SettingsPage } from "../SettingsPage";

export const Families = () => {

    const { data: families } = useFamilies();

    return (
        <SettingsPage title="Families" breadcrumbs={[{ text: "Families", route: "/settings/families" }]} actions={[<Link key="add" className="btn btn-primary" to="/settings/families/add"><FontAwesomeIcon icon="plus" size="xs" />Add Family</Link>]}>
            <Table className="section" striped hover>
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {families && families.map((f) => (
                        <tr key={f.id}>
                            <td>{f.name}</td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </SettingsPage>
    )
}
