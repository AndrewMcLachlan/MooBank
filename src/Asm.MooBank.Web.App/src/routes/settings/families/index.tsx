import { createFileRoute } from "@tanstack/react-router";
import { IconLinkButton, SectionTable } from "@andrewmclachlan/moo-ds";
import { useFamilies } from "services";
import { SettingsPage } from "../-components/SettingsPage";
import { useNavigate } from "@tanstack/react-router";

export const Route = createFileRoute("/settings/families/")({
    component: Families,
});

function Families() {

    const { data: families } = useFamilies();

    const navigate = useNavigate();

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
                        <tr key={f.id} className="clickable" onClick={() => navigate({ to: `/settings/families/${f.id}` })}>
                            <td>{f.name}</td>
                        </tr>
                    ))}
                </tbody>
            </SectionTable>
        </SettingsPage>
    )
}
