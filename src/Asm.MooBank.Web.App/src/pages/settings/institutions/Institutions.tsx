import { IconLinkButton, SectionTable } from "@andrewmclachlan/mooapp";
import { Table } from "react-bootstrap";
import { useNavigate } from "react-router-dom";
import { useInstitutions } from "services";
import { SettingsPage } from "../SettingsPage";
import { institutionTypeOptions } from "models";

export const Institutions = () => {

    const { data: institutions } = useInstitutions();

    const navigate = useNavigate();

    return (
        <SettingsPage title="Institutions" breadcrumbs={[{ text: "Institutions", route: "/settings/institutions" }]} actions={[<IconLinkButton variant="primary" key="add" to="/settings/institutions/add" icon="plus">Add Institution</IconLinkButton>]}>
            <SectionTable striped hover>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th>Type</th>
                    </tr>
                </thead>
                <tbody>
                    {institutions && institutions.map((f) => (
                        <tr key={f.id} className="clickable" onClick={() => navigate(`/settings/institutions/${f.id}`)}>
                            <td>{f.name}</td>
                            <td>{institutionTypeOptions.find(i => i.value === f.institutionType)?.label}</td>
                        </tr>
                    ))}
                </tbody>
            </SectionTable>
        </SettingsPage>
    );
};
