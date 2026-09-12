import { describe, it, expect, beforeEach, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { Account } from "api/types.gen";

// Stable spies shared with the mocked hook modules (hoisted above the vi.mock calls).
const mocks = vi.hoisted(() => ({
    createMutate: vi.fn(),
    updateMutate: vi.fn(),
    navigate: vi.fn(),
}));

vi.mock("../-hooks/useCreateBillAccount", () => ({
    useCreateBillAccount: () => ({ mutateAsync: mocks.createMutate, isPending: false }),
}));
vi.mock("../-hooks/useUpdateBillAccount", () => ({
    useUpdateBillAccount: () => ({ mutateAsync: mocks.updateMutate, isPending: false }),
}));
vi.mock("hooks/useUser", () => ({
    useUser: () => ({ data: { currency: "AUD" } }),
}));
vi.mock("@tanstack/react-router", () => ({
    useNavigate: () => mocks.navigate,
}));

import { BillAccountForm } from "./BillAccountForm";

const account: Account = {
    id: "acc-1",
    name: "AGL Electricity",
    description: "Off-peak plan",
    accountNumber: "ELEC001",
    currency: "AUD",
    utilityType: "Electricity",
    shareWithFamily: true,
} as Account;

const input = (id: string) => document.querySelector<HTMLInputElement>(`#${id}`)!;
const textArea = (id: string) => document.querySelector<HTMLTextAreaElement>(`#${id}`)!;

beforeEach(() => {
    mocks.createMutate.mockReset().mockResolvedValue(undefined);
    mocks.updateMutate.mockReset().mockResolvedValue(undefined);
    mocks.navigate.mockReset();
});

describe("BillAccountForm", () => {
    describe("editing an existing account", () => {
        it("seeds the fields from the account", () => {
            render(<BillAccountForm account={account} />);

            expect(input("name")).toHaveValue("AGL Electricity");
            expect(textArea("description")).toHaveValue("Off-peak plan");
            expect(input("accountNumber")).toHaveValue("ELEC001");
            expect(input("utilityType")).toHaveValue("Electricity");
            expect(input("currency")).toHaveValue("AUD");
        });

        it("locks the utility type and the currency", () => {
            render(<BillAccountForm account={account} />);

            expect(input("utilityType")).toBeDisabled();
            expect(input("currency")).toBeDisabled();
        });

        it("routes the submission to the update mutation, not create", async () => {
            const user = userEvent.setup();
            render(<BillAccountForm account={account} />);

            await user.clear(input("name"));
            await user.type(input("name"), "AGL Electric");
            await user.click(screen.getByRole("button", { name: "Save" }));

            expect(mocks.updateMutate).toHaveBeenCalledTimes(1);
            expect(mocks.updateMutate).toHaveBeenCalledWith("acc-1", {
                name: "AGL Electric",
                description: "Off-peak plan",
                accountNumber: "ELEC001",
                shareWithFamily: true,
            });
            expect(mocks.createMutate).not.toHaveBeenCalled();
        });

        it("sends a cleared description as null rather than a blank string", async () => {
            const user = userEvent.setup();
            render(<BillAccountForm account={account} />);

            await user.clear(textArea("description"));
            await user.click(screen.getByRole("button", { name: "Save" }));

            expect(mocks.updateMutate).toHaveBeenCalledWith("acc-1", expect.objectContaining({ description: null }));
        });

        it("returns to the account once saved", async () => {
            const user = userEvent.setup();
            render(<BillAccountForm account={account} />);

            await user.click(screen.getByRole("button", { name: "Save" }));

            expect(mocks.navigate).toHaveBeenCalledWith({ to: "/bills/accounts/acc-1" });
        });
    });

    describe("creating an account", () => {
        it("offers the utility type and the currency for selection", () => {
            render(<BillAccountForm />);

            expect(screen.getByPlaceholderText("Select a utility type...")).toBeEnabled();
            expect(input("utilityType")).not.toBeDisabled();
        });

        it("routes the submission to the create mutation, not update", async () => {
            const user = userEvent.setup();
            render(<BillAccountForm />);

            await user.type(input("name"), "Origin Gas");
            await user.type(input("accountNumber"), "GAS001");
            await user.click(screen.getByRole("button", { name: "Create" }));

            expect(mocks.createMutate).toHaveBeenCalledTimes(1);
            expect(mocks.createMutate).toHaveBeenCalledWith(expect.objectContaining({ name: "Origin Gas", accountNumber: "GAS001", currency: "AUD", shareWithFamily: true }));
            expect(mocks.updateMutate).not.toHaveBeenCalled();
        });
    });
});
