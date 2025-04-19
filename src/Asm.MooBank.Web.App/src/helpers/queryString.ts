const addOrReplaceQueryString = (params: URLSearchParams, key: string, value: string) => {

    params.delete(key);
    params.append(key, value);
    const queryString = params.toString();
    const newUrl = window.location.origin + window.location.pathname + (queryString === "" ? "" : `?${queryString}`);

    window.history.replaceState({ path: newUrl }, "", newUrl);
}