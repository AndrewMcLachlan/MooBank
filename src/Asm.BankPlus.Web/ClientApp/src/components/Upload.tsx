import "./Upload.scss";

import React, { useState } from "react";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";

export const Upload: React.FC<UploadProps> = (props) => {

    const dragEvents = useDragEvents(props);

    return (
        <section className="upload">
            <div className="upload-box" onDragEnter={dragEvents.dragEnter} onDragLeave={dragEvents.dragLeave} onDragOver={dragEvents.dragOver} onDrop={dragEvents.drop} >
                <FontAwesomeIcon icon={["fas", "upload"]} />
                <label>
                    <div>
                        <span>Drag a file here</span>
                        <span>or click to browse</span>
                    </div>
                    <input type="file" accept="text/csv" multiple={props.allowMultiple} onChange={dragEvents.filesChanged} />
                </label>
            </div>
            <ul>
                {dragEvents.files.map(f =>
                    <li>{f.name}</li>
                )}
            </ul>
        </section>
    );
}

Upload.displayName = "Upload";
Upload.defaultProps = {
    allowMultiple: false,
}

const useDragEvents = (props: UploadProps) => {

    const [isDragging, setIsDragging] = useState(false);
    const [files, setFiles] = useState([] as File[]);

    const stopEvent = (e: React.SyntheticEvent<any>) => {
        e.preventDefault();
        e.stopPropagation();
    };

    const onFilesAdded = (currentFiles: File[], newFiles: File[]) => {
        props.onFilesAdded && props.onFilesAdded({ currentFiles, newFiles });
    };

    const filesChanged = (e: React.ChangeEvent<HTMLInputElement>) => {
        const currentFiles = files.concat();
        const newFiles = [];

        if (props.allowMultiple) {

            for (let i = 0; i < e.currentTarget.files.length; i++) {
                newFiles.push(e.currentTarget.files[i]);
            }

            onFilesAdded(currentFiles, newFiles);
            setFiles(files.concat(newFiles));
        }
        else {
            onFilesAdded(currentFiles, [e.currentTarget.files[0]]);
            setFiles([e.currentTarget.files[0]]);
        }
    };

    const drop = (e: React.DragEvent<HTMLDivElement>) => {
        stopEvent(e);

        setIsDragging(false);

        if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {

            const currentFiles = files.concat();
            const newFiles = [];

            if (props.allowMultiple) {

                for (let i = 0; i < e.dataTransfer.files.length; i++) {
                    newFiles.push(e.dataTransfer.files[i]);
                }

                onFilesAdded(currentFiles, newFiles);
                setFiles(files.concat(newFiles));
            }
            else {
                onFilesAdded(currentFiles, [e.dataTransfer.files[0]]);
                setFiles([e.dataTransfer.files[0]]);
            }

            e.dataTransfer.clearData();
        }
    };

    const dragEnter = (e: React.DragEvent<HTMLDivElement>) => {
        stopEvent(e);

        if (e.dataTransfer.items && e.dataTransfer.items.length > 0) {
            setIsDragging(true);
        }
    };

    const dragLeave = (e: React.DragEvent<HTMLDivElement>) => {
        stopEvent(e);

        setIsDragging(false);
    };

    const dragOver = (e: React.DragEvent<HTMLDivElement>) => {
        stopEvent(e);
    };

    return {
        drop,
        dragEnter,
        dragLeave,
        dragOver,
        isDragging,
        filesChanged,
        files,
    };
}

export interface UploadProps {
    allowMultiple?: boolean;
    onFilesAdded?: (e: FilesAddedEvent) => void;
}

export interface FilesAddedEvent {
    currentFiles: File[];
    newFiles: File[];
}