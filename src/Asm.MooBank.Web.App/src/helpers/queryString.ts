export const addOrReplaceQueryString = (params: URLSearchParams, key: string, value: string) => {

    params.delete(key);
    params.append(key, value);
    const queryString = params.toString();
    const newUrl = window.location.origin + window.location.pathname + (queryString === "" ? "" : `?${queryString}`);

    window.history.replaceState({ path: newUrl }, "", newUrl);
}

export const cleanQueryString = (params: URLSearchParams, key: string) =>{
    
    params.delete(key);
    const queryString = params.toString();
    const newUrl = window.location.origin + window.location.pathname + (queryString === "" ? "" : `?${queryString}`);

    window.history.replaceState({ path: newUrl }, "", newUrl);
}
