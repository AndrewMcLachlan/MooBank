import { describe, it, expect, vi, beforeEach } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { RetirementPlan } from "api/types.gen";

const updateAsync = vi.fn().mockResolvedValue({});

vi.mock("../-retirement-hooks/useUpdateRetirementPlan", () => ({
    useUpdateRetirementPlan: () => ({ updateAsync, isPending: false }),
}));

vi.mock("hooks/useAccounts", () => ({
    useAccounts: () => ({ data: [
        { id: selfAccountId, name: "AustralianSuper (Andy)", accountType: "Superannuation" },
        { id: spouseAccountId, name: "AustralianSuper (Margo)", accountType: "Superannuation" },
    ] }),
}));

vi.mock("../-retirement-hooks/useFamilyMembers", () => ({
    useFamilyMembers: () => ({ members: [
        { id: selfUserId, firstName: "Andy", lastName: "M", emailAddress: "a@example.com", accounts: [selfAccountId] },
        { id: spouseUserId, firstName: "Margo", lastName: "M", emailAddress: "m@example.com", accounts: [spouseAccountId] },
    ] }),
}));

const selfUserId = "11111111-1111-1111-1111-111111111111";
const spouseUserId = "22222222-2222-2222-2222-222222222222";
const selfAccountId = "33333333-3333-3333-3333-333333333333";
const spouseAccountId = "44444444-4444-4444-4444-444444444444";
const selfMemberId = "55555555-5555-5555-5555-555555555555";

const plan = (): RetirementPlan => ({
    id: "66666666-6666-6666-6666-666666666666",
    name: "Retirement",
    expectedReturnRate: 0.065,
    inflationRate: 0.025,
    superGuaranteeRate: 0.12,
    contributionsTaxRate: 0.15,
    lifeExpectancy: 85,
    targetRetirementIncome: 138_000,
    preRetirementSwitchYears: 2,
    cashReturnRate: 0.03,
    createdUtc: "2026-01-01T00:00:00Z",
    updatedUtc: "2026-01-01T00:00:00Z",
    members: [
        {
            id: selfMemberId, userId: selfUserId, name: "Andy", currentAge: 47, currentIncome: 231_000,
            salarySacrifice: 1_000, retirementAge: 67, growthStrategy: "Growth", annualFees: 0,
            insurancePremium: 0, instrumentIds: [selfAccountId],
        },
    ],
});

const { RetirementSettingsModal } = await import("./RetirementSettingsModal");

/**
 * What the settings form actually puts on the wire.
 *
 * Saving after adding a person failed in the binder rather than in validation, which means the
 * payload carried a value the server could not read at all. These pin the payload itself, because
 * that is the thing that was wrong — no amount of server-side validation helps if the request never
 * gets that far.
 */
describe("saving the retirement plan", () => {
    beforeEach(() => updateAsync.mockClear());

    const save = async () => {
        await userEvent.click(screen.getByRole("button", { name: /^save$/i }));
        return updateAsync.mock.calls[0]?.[1];
    };

    it("sends the existing member unchanged", async () => {
        render(<RetirementSettingsModal plan={plan()} currencyCode="AUD" show onHide={() => { }} />);

        const sent = await save();

        expect(sent).toBeDefined();
        expect(sent.members).toHaveLength(1);
        expect(sent.members[0].id).toBe(selfMemberId);
        expect(sent.members[0].userId).toBe(selfUserId);
    });

    /**
     * The form must not put an unchosen person on the wire at all. One carries no readable id, so the
     * request would be refused in the binder before any rule could say which row is unfinished.
     */
    it("will not save while anyone has no person chosen", async () => {
        render(<RetirementSettingsModal plan={plan()} currencyCode="AUD" show onHide={() => { }} />);

        await userEvent.click(screen.getByRole("button", { name: /add person/i }));

        const saveButton = screen.getByRole("button", { name: /^save$/i });
        expect(saveButton).toBeDisabled();
        expect(screen.getByText(/choose a person for everyone/i)).toBeInTheDocument();

        await userEvent.click(saveButton);
        expect(updateAsync).not.toHaveBeenCalled();
    });

    it("saves again once the missing person is chosen", async () => {
        render(<RetirementSettingsModal plan={plan()} currencyCode="AUD" show onHide={() => { }} />);

        await userEvent.click(screen.getByRole("button", { name: /add person/i }));

        const selects = screen.getAllByRole("combobox").filter(s => s.getAttribute("name")?.endsWith(".userId"));
        await userEvent.selectOptions(selects[1], spouseUserId);

        expect(screen.getByRole("button", { name: /^save$/i })).toBeEnabled();
        expect(screen.queryByText(/choose a person for everyone/i)).not.toBeInTheDocument();
    });

    it("sends an added person with the user chosen for them and no id of their own", async () => {
        render(<RetirementSettingsModal plan={plan()} currencyCode="AUD" show onHide={() => { }} />);

        await userEvent.click(screen.getByRole("button", { name: /add person/i }));

        const selects = screen.getAllByRole("combobox").filter(s => s.getAttribute("name")?.endsWith(".userId"));
        expect(selects).toHaveLength(2);
        await userEvent.selectOptions(selects[1], spouseUserId);

        const sent = await save();

        expect(sent).toBeDefined();
        expect(sent.members).toHaveLength(2);
        expect(sent.members[1].userId).toBe(spouseUserId);
        // Nothing the server would try, and fail, to read as a member key.
        expect(sent.members[1].id == null).toBe(true);
    });
});
