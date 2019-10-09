import { State } from "store/state";
import { HttpErrorCodes, ProblemDetails } from "./HttpClient";

export class ServiceBase {

    protected state: State;

    constructor(state: State) {
        this.state = state;
    }

    protected handleProblemDetails(problemDetails: ProblemDetails) {
        throw new Error(problemDetails.title);
    }

    protected async handleError(response: Response): Promise<void> {
        switch (response.status as HttpErrorCodes) {
            case HttpErrorCodes.ServiceUnavailable:
                throw new Error(`${this.state.app.appName} is currently unavailable`);
            default:
                const problemDetails: ProblemDetails = await response.json();
                this.handleProblemDetails(problemDetails);
        }

        return Promise.resolve();
    }
}