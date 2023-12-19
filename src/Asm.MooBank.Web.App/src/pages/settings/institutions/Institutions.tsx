import { IconLinkButton } from "@andrewmclachlan/mooapp";
import { Table } from "react-bootstrap";
import { useInstitutions } from "services";
import { SettingsPage } from "../SettingsPage";
import { useNavigate } from "react-router-dom";

export const Institutions = () => {

    const { data: institutions, isLoading } = useInstitutions();

    const navigate = useNavigate();

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }]} actions={[<IconLinkButton variant="primary" key="add" to="/settings/institutions/add" icon="plus">Add Institution</IconLinkButton>]}>
            <Table className="section" striped hover>
                <thead>
                    <tr>
                        <th>Name</th>
                    </tr>
                </thead>
                <tbody>
                    {institutions && institutions.map((f) => (
                        <tr key={f.id} className="clickable" onClick={() => navigate(`/settings/institutions/${f.id}`)}>
                            <td>{f.name}</td>
                        </tr>
                    ))}
                </tbody>
            </Table>
        </SettingsPage>
    );
};
