export const cleanQueryString = (params: URLSearchParams, key: string) =>{

    params.delete(key);
    const queryString = params.toString();
    const newUrl = window.location.origin + window.location.pathname + (queryString === "" ? "" : `?${queryString}`);

    window.history.replaceState({ path: newUrl }, "", newUrl);
}
