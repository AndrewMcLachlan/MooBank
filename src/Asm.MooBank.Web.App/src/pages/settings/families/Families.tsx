import { IconLinkButton, SectionTable } from "@andrewmclachlan/mooapp";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { Link } from "react-router";
import { useFamilies } from "services";
import { SettingsPage } from "../SettingsPage";

export const Families = () => {

    const { data: families } = useFamilies();

    return (
        <SettingsPage title="Families" breadcrumbs={[{ text: "Families", route: "/settings/families" }]} actions={[<IconLinkButton key="add" variant="primary" to="/settings/families/add" icon="plus">Add Family</IconLinkButton>]}>
            <SectionTable striped hover>
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
            </SectionTable>
        </SettingsPage>
    )
}
