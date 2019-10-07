import "./ComboBox.scss";

import React, { PropsWithChildren, useState, forwardRef } from "react";

export const ComboBox = forwardRef<any, ComboBoxProps>((props, ref) => {

    const comboBox = useComboBox(props);

    const value = (comboBox.selectedItem && comboBox.selectedItem[props.textField]) || comboBox.text;

    return (
        <div className="combo-box" hidden={props.hidden} ref={ref}>
            <input type="text" onChange={comboBox.onChange} value={value} tabIndex={1} />
            <div className="cb-arrow" onClick={comboBox.showHideItems}>
                <i className="fa fa-chevron-down" />
            </div>
            {comboBox.items && comboBox.items.length > 0 &&
                <ol className="cb-list">
                    {comboBox.items.map((i) =>
                        <ComobBoxItem key={i[props.valueField]} onSelected={comboBox.onItemSelected} item={i} textField={props.textField} />
                    )}
                </ol>
            }
        </div>
    );
});

const ComobBoxItem: React.FC<ComboBoxItemProps> = (props) => {

    const click = (e: React.MouseEvent<HTMLLIElement>) => {
        e.preventDefault();
        e.stopPropagation();

        props.onSelected(props.item);
    }

    return (<li onClick={click} tabIndex={2}>{props.item[props.textField]}</li>);
}

function useComboBox(props: ComboBoxProps) {

    const [items, setItems] = useState([]);
    const [text, setText] = useState("");
    const [selectedItem, setSelectedItem] = useState(props.selectedItem);
    const [newItem, setNewItem] = useState(null as any);

    const onChange = (e: React.ChangeEvent<HTMLInputElement>) => {

        setText(e.currentTarget.value);
        setSelectedItem(null);

        if (props.search) {
            // TODO: Debounce
            setItems(props.search(e.currentTarget.value));
        }
        else {

            let tempItems = props.items.filter((i) => i[props.textField].toLowerCase().startsWith(e.currentTarget.value.toLowerCase()));

            if (props.allowAdd && !props.items.some((i) => i[props.textField].toLowerCase() === e.currentTarget.value.toLowerCase())) {

                let addItem = newItem ? newItem : {};

                addItem[props.textField] = e.currentTarget.value + " (Add new)";

                tempItems.unshift(addItem);

                setNewItem(addItem);
            }

            setItems(tempItems);
        }
    }

    const onItemSelected = (item: any) => {

        if (item === newItem) {
            item[props.textField] = (item[props.textField] as string).replace(/\(Add new\)$/, "");
            props.onAdd && props.onAdd(item[props.textField] as string);
        }

        setSelectedItem(item);
        props.onSelected && props.onSelected(item);
        setItems([]);
    }

    const showHideItems = (e: React.MouseEvent<any>) => {

        e.preventDefault();
        e.stopPropagation();

        if (items.length > 1) {
            setItems([]);
        }
        else {
            setItems(props.items);
        }
    }

    return {
        onChange,
        onItemSelected,
        items,
        selectedItem,
        showHideItems,
        text,
    };
}

export interface ComboBoxProps {
    search?: (input: string) => any[];
    onAdd?: (name: string) => void;
    onSelected?: (item: any) => void;
    allowAdd?: boolean;
    textField: string;
    valueField: string;
    items?: any[];
    hidden?: boolean;
    selectedItem?: any;
}

export interface ComboBoxItemProps {
    onSelected: (item: any) => void;
    textField: string;
    item: any;
}