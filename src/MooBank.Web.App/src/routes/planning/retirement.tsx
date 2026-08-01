import { createFileRoute } from "@tanstack/react-router";
import { IconButton, SpinnerContainer } from "@andrewmclachlan/moo-ds";
import { Sliders } from "@andrewmclachlan/moo-icons";
import { useEffect, useMemo, useRef, useState } from "react";
import { useRetirementPlans } from "./-retirement-hooks/useRetirementPlans";
import { useRetirementPlan } from "./-retirement-hooks/useRetirementPlan";
import { useRetirementProjection } from "./-retirement-hooks/useRetirementProjection";
import { useUpdateRetirementPlan } from "./-retirement-hooks/useUpdateRetirementPlan";
import { useDebounced } from "./-retirement-hooks/useDebounced";
import { useUser } from "hooks/useUser";
import { RetirementPage } from "./-retirement-components/RetirementPage";
import { RetirementOutlook } from "./-retirement-components/RetirementOutlook";
import { RetirementChart } from "./-retirement-components/RetirementChart";
import { RetirementIncomeChart } from "./-retirement-components/RetirementIncomeChart";
import { RetirementMembersTable } from "./-retirement-components/RetirementMembersTable";
import { RetirementAssumptionsNote } from "./-retirement-components/RetirementAssumptionsNote";
import { RetirementSettingsModal } from "./-retirement-components/RetirementSettingsModal";
import { RetirementTweaks } from "./-retirement-components/RetirementTweaks";
import { CreateRetirementPlan } from "./-retirement-components/CreateRetirementPlan";
import { applyDraftToPlan, emptyDraft, pruneDraft, withPlanValue } from "./-retirement-utils/tweaks";
import { householdKey, projectionMatchesDraft } from "./-retirement-utils/retirementSync";
import type { RetirementProjectionOverrides } from "api/types.gen";

export const Route = createFileRoute("/planning/retirement")({
    component: Retirement,
});

function Retirement() {
    const [selectedPlanId, setSelectedPlanId] = useState<string | null>(null);
    const [editOpen, setEditOpen] = useState(false);

    // The tweak sliders' working copy: only the values actually moved, so an untouched slider reads
    // through to the plan and follows it when the settings are edited. Lives only here, so a
    // refresh loses it.
    const [draft, setDraft] = useState<RetirementProjectionOverrides>(emptyDraft);

    const { data: plans, isLoading: plansLoading } = useRetirementPlans();
    const { data: user } = useUser();

    // One plan is supported for now, so settle on it rather than offering a picker. Derived rather
    // than synchronised in an effect, so there is no render where the list has arrived but nothing
    // is selected.
    const planId = selectedPlanId ?? plans?.[0]?.id ?? null;

    const { data: plan } = useRetirementPlan(planId);
    const { updateAsync, isPending: saving } = useUpdateRetirementPlan();

    // Only the settled draft reaches the query, so dragging a slider does not fire a request a frame.
    // Overrides for members the plan no longer has are dropped, so removing someone in settings
    // cannot leave a stale tweak driving the projection.
    const scoped = useMemo(() => plan ? pruneDraft(draft, plan) : draft, [draft, plan]);
    const settledDraft = useDebounced(scoped, 300);
    const { data: projection, isFetching: projectionLoading } = useRetirementProjection(planId, settledDraft);

    const currencyCode = user?.currency ?? "AUD";

    /**
     * Leaving someone out changes the household, and with it the balance the target income was
     * solved against — a target a couple could sustain will outlive one person's savings. The income
     * is re-solved for the age already chosen, which is the same rule the two sliders follow when
     * either is moved: the horizon is what the household holds to, and the income follows from it.
     *
     * Gated on the projection actually describing the household the draft is asking for, rather than
     * on the request having had time to finish. The draft reaches the query debounced, so for a
     * moment the projection on screen still belongs to the previous set of people — and an earlier
     * version of this read the summary in exactly that moment. It re-solved against the household it
     * was leaving, which changed nothing visible, marked the work done, and never looked again; then
     * putting the person back solved against the one-person figures still on screen. Comparing who
     * is in the result against who is wanted cannot drift that way.
     *
     * It cannot chase its own tail either: a balance at retirement is settled before any drawdown
     * begins, so it does not move when the target income does.
     */
    const solvedFor = useRef<string | null>(null);

    useEffect(() => {
        if (!plan || !projection) return;

        const planMemberIds = plan.members.map(m => m.id);
        const projectedMemberIds = projection.members.map(m => m.memberId);

        // Still showing the previous household; wait for the one that was asked for.
        if (!projectionMatchesDraft(planMemberIds, scoped.excludedMemberIds, projectedMemberIds)) return;

        // Keyed on the horizon as well as the household, both read from the result rather than the
        // draft, so the income is re-solved whenever either changes — and never against a projection
        // that has not caught up.
        const household = `${householdKey(projectedMemberIds)}@${projection.summary.lifeExpectancyYear}`;

        // The first matching projection sets the baseline rather than re-solving against it.
        if (solvedFor.current === null || solvedFor.current === household) {
            solvedFor.current = household;
            return;
        }

        solvedFor.current = household;

        // The server's own figure, solved against the projection rather than estimated from the
        // closing balance: it accounts for the fees still being charged and for the pension sharing
        // the load, which an annuity cannot, and which it overstated by a tenth or more.
        const income = projection.summary.sustainableIncomeInTodaysDollars;
        if (income <= 0) return;

        setDraft(current => withPlanValue(current, plan, "targetRetirementIncome", income));
    }, [plan, projection, scoped]);

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
        if (!plan) return;

        await updateAsync(plan.id, applyDraftToPlan(scoped, plan));

        // The tweaks are the plan now, so drop the working copy and go back to reading it.
        setDraft(emptyDraft);
    };

    return (
        <RetirementPage plan={plan} actions={actions}>
            <RetirementOutlook projection={projection} currencyCode={currencyCode} loading={projectionLoading && !projection} />

            <div className="retirement-charts">
                <RetirementChart years={projection?.years ?? []} currencyCode={currencyCode} pensionStartsBelow={projection?.summary.pensionStartsBelowInTodaysDollars} />
                <RetirementIncomeChart years={projection?.years ?? []} currencyCode={currencyCode} />
            </div>

            {plan && plan.members.length > 0 && (
                <RetirementTweaks
                    plan={plan}
                    draft={scoped}
                    summary={projection?.summary}
                    saving={saving}
                    currencyCode={currencyCode}
                    onChange={setDraft}
                    onReset={() => setDraft(emptyDraft)}
                    onLockIn={lockIn}
                />
            )}

            <RetirementMembersTable members={projection?.members ?? []} currencyCode={currencyCode} />

            <RetirementAssumptionsNote plan={plan} />

            {/* Mounted only while open — see the note on the forecast page. */}
            {editOpen && <RetirementSettingsModal plan={plan} summary={projection?.summary} currencyCode={currencyCode} show={editOpen} onHide={() => setEditOpen(false)} />}
        </RetirementPage>
    );
}
