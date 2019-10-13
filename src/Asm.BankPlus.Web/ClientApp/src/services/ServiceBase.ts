import { State } from "store/state";
import { HttpStatusCodes, ProblemDetails } from "./HttpClient";

export class ServiceBase {

    protected state: State;

    constructor(state: State) {
        this.state = state;
    }

    protected handleProblemDetails(problemDetails: ProblemDetails) {
        throw new Error(problemDetails.title);
    }

    protected async handleError(response: Response): Promise<void> {
        switch (response.status as HttpStatusCodes) {
            case HttpStatusCodes.ServiceUnavailable:
                throw new Error(`${this.state.app.appName} is currently unavailable`);
            default:
                try {
                const problemDetails: ProblemDetails = await response.json();
                this.handleProblemDetails(problemDetails);
                }
                catch(error) {
                    throw new Error(`${this.state.app.appName} is currently unavailable`);
                }
        }

        return Promise.resolve();
    }
}