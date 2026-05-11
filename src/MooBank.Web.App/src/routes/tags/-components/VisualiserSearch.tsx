import { useEffect, useState } from "react";
import { useDebouncedCallback } from "use-debounce";

interface Props {
    value: string;
    onChange: (next: string) => void;
    placeholder?: string;
    matchCount?: number;
}

export const VisualiserSearch: React.FC<Props> = ({ value, onChange, placeholder = "Search tags…", matchCount }) => {
    const [local, setLocal] = useState(value);

    useEffect(() => { setLocal(value); }, [value]);

    const debounced = useDebouncedCallback((next: string) => onChange(next), 150);

    return (
        <div className="visualiser-search" role="search">
            <input
                type="search"
                role="searchbox"
                className="visualiser-search-input"
                value={local}
                onChange={(e) => {
                    setLocal(e.target.value);
                    debounced(e.target.value);
                }}
                placeholder={placeholder}
                aria-label="Search tags"
            />
            <span className="visualiser-search-count" aria-live="polite">
                {value.length > 0 && matchCount !== undefined ? `${matchCount} match${matchCount === 1 ? "" : "es"}` : ""}
            </span>
        </div>
    );
};
