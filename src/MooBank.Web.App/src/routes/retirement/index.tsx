import { createFileRoute } from "@tanstack/react-router";
import { IconButton, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { Sliders } from "@andrewmclachlan/moo-icons";
import { useState } from "react";
import { useRetirementPlans } from "./-hooks/useRetirementPlans";
import { useRetirementPlan } from "./-hooks/useRetirementPlan";
import { useRetirementProjection } from "./-hooks/useRetirementProjection";
import { useUser } from "hooks/useUser";
import { RetirementPage } from "./-components/RetirementPage";
import { RetirementOutlook } from "./-components/RetirementOutlook";
import { RetirementChart } from "./-components/RetirementChart";
import { RetirementMembersTable } from "./-components/RetirementMembersTable";
import { RetirementAssumptionsNote } from "./-components/RetirementAssumptionsNote";
import { RetirementSettingsModal } from "./-components/RetirementSettingsModal";
import { CreateRetirementPlan } from "./-components/CreateRetirementPlan";

export const Route = createFileRoute("/retirement/")({
    component: Retirement,
});

function Retirement() {
    const [selectedPlanId, setSelectedPlanId] = useState<string | null>(null);
    const [editOpen, setEditOpen] = useState(false);

    const { data: plans, isLoading: plansLoading } = useRetirementPlans();
    const { data: user } = useUser();

    // Fall back to the most recently updated plan until one is explicitly chosen. Derived rather
    // than synchronised in an effect, so there is no render where the list has arrived but nothing
    // is selected yet.
    const planId = selectedPlanId ?? plans?.[0]?.id ?? null;

    const { data: plan } = useRetirementPlan(planId);
    const { data: projection, isFetching: projectionLoading } = useRetirementProjection(planId);

    const currencyCode = user?.currency ?? "AUD";

    if (plansLoading) {
        return (
            <RetirementPage>
                <SpinnerContainer />
            </RetirementPage>
        );
    }

    if (plans && plans.length === 0) {
        return (
            <RetirementPage>
                <CreateRetirementPlan onPlanCreated={setSelectedPlanId} />
            </RetirementPage>
        );
    }

    const actions = plan ? [
        <IconButton badge key="edit-settings" variant="primary" icon={Sliders} onClick={() => setEditOpen(true)}>Edit Plan</IconButton>
    ] : [];

    return (
        <RetirementPage plan={plan} actions={actions}>
            <RetirementOutlook projection={projection} currencyCode={currencyCode} loading={projectionLoading} />

            <RetirementChart years={projection?.years ?? []} currencyCode={currencyCode} />

            <RetirementMembersTable members={projection?.members ?? []} currencyCode={currencyCode} />

            <RetirementAssumptionsNote plan={plan} />

            <RetirementSettingsModal plan={plan} currencyCode={currencyCode} show={editOpen} onHide={() => setEditOpen(false)} />
        </RetirementPage>
    );
}
