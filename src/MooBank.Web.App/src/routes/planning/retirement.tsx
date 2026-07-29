import { createFileRoute } from "@tanstack/react-router";
import { IconButton, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { Sliders } from "@andrewmclachlan/moo-icons";
import { useMemo, useState } from "react";
import { useRetirementPlans } from "./-retirement-hooks/useRetirementPlans";
import { useRetirementPlan } from "./-retirement-hooks/useRetirementPlan";
import { useRetirementProjection } from "./-retirement-hooks/useRetirementProjection";
import { useUpdateRetirementPlan } from "./-retirement-hooks/useUpdateRetirementPlan";
import { useDebounced } from "./-retirement-hooks/useDebounced";
import { useUser } from "hooks/useUser";
import { RetirementPage } from "./-retirement-components/RetirementPage";
import { RetirementOutlook } from "./-retirement-components/RetirementOutlook";
import { RetirementChart } from "./-retirement-components/RetirementChart";
import { RetirementMembersTable } from "./-retirement-components/RetirementMembersTable";
import { RetirementAssumptionsNote } from "./-retirement-components/RetirementAssumptionsNote";
import { RetirementSettingsModal } from "./-retirement-components/RetirementSettingsModal";
import { RetirementTweaks } from "./-retirement-components/RetirementTweaks";
import { CreateRetirementPlan } from "./-retirement-components/CreateRetirementPlan";
import { applyDraftToPlan, draftFromPlan, isDirty } from "./-retirement-utils/tweaks";
import type { RetirementProjectionOverrides } from "api/types.gen";

export const Route = createFileRoute("/planning/retirement")({
    component: Retirement,
});

function Retirement() {
    const [selectedPlanId, setSelectedPlanId] = useState<string | null>(null);
    const [editOpen, setEditOpen] = useState(false);

    // The tweak sliders' working copy. Null means "as saved"; it is seeded from the plan the first
    // time a slider moves, and lives only here, so a refresh loses it.
    const [draft, setDraft] = useState<RetirementProjectionOverrides | null>(null);

    const { data: plans, isLoading: plansLoading } = useRetirementPlans();
    const { data: user } = useUser();

    // One plan is supported for now, so settle on it rather than offering a picker. Derived rather
    // than synchronised in an effect, so there is no render where the list has arrived but nothing
    // is selected.
    const planId = selectedPlanId ?? plans?.[0]?.id ?? null;

    const { data: plan } = useRetirementPlan(planId);
    const { updateAsync, isPending: saving } = useUpdateRetirementPlan();

    // Only the settled draft reaches the query, so dragging a slider does not fire a request a frame.
    const settledDraft = useDebounced(draft, 300);
    const { data: projection, isFetching: projectionLoading } = useRetirementProjection(planId, settledDraft ?? undefined);

    const currencyCode = user?.currency ?? "AUD";

    // Page pushes actions into the layout by reference, so a fresh array on every render sets the
    // context every render, which re-renders and builds another array. Memoised, and declared
    // above the early returns so the hook order stays fixed.
    const actions = useMemo(() => plan ? [
        <IconButton badge key="edit-settings" variant="primary" icon={Sliders} onClick={() => setEditOpen(true)}>Edit Plan</IconButton>
    ] : [], [plan]);

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

    const lockIn = async () => {
        if (!plan || !draft) return;

        await updateAsync(plan.id, applyDraftToPlan(draft, plan));

        // The tweaks are the plan now, so drop the working copy and go back to reading it.
        setDraft(null);
    };

    return (
        <RetirementPage plan={plan} actions={actions}>
            <RetirementOutlook projection={projection} currencyCode={currencyCode} loading={projectionLoading && !projection} />

            <RetirementChart years={projection?.years ?? []} currencyCode={currencyCode} />

            {plan && plan.members.length > 0 && (
                <RetirementTweaks
                    plan={plan}
                    draft={draft ?? draftFromPlan(plan)}
                    dirty={!!draft && isDirty(draft, plan)}
                    saving={saving}
                    currencyCode={currencyCode}
                    onChange={setDraft}
                    onReset={() => setDraft(null)}
                    onLockIn={lockIn}
                />
            )}

            <RetirementMembersTable members={projection?.members ?? []} currencyCode={currencyCode} />

            <RetirementAssumptionsNote plan={plan} />

            <RetirementSettingsModal plan={plan} currencyCode={currencyCode} show={editOpen} onHide={() => setEditOpen(false)} />
        </RetirementPage>
    );
}
