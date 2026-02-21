import { createFileRoute } from "@tanstack/react-router";
import { ReactNode } from "react";

import type { Tag } from "api/types.gen";

import { useTagsHierarchy } from "services";
import { TagsPage } from "./-components/TagsPage";
import { Container } from "@andrewmclachlan/moo-ds";
import classNames from "classnames";

export const Route = createFileRoute("/tags/visualiser")({
    component: Visualiser,
});

function Visualiser() {


    const { data: tagHierarchy } = useTagsHierarchy();

    if (!tagHierarchy) return null;

    return (
        <TagsPage>
            <Container fluid className="visualiser">
                {getTree(tagHierarchy.tags)}
            </Container>
        </TagsPage>
    )

}

let colourIndex = 0;

const nextColour = () => {
    colourIndex++;
    if (colourIndex >= 20) colourIndex = 0;
    return `rainbow rainbow-${colourIndex}`;
}

const getTree = (tags: Tag[]): ReactNode => {

    const bigTags = tags.filter(tag => tag.tags.length > 3);

    const smallTags = tags.filter(tag => tag.tags.length <= 3);

    const results = [];

    results.push(
        bigTags.map(tag => {
            colourIndex = 0;
            return (
                <div className="tf-tree" key={tag.id}>
                    <ul>
                        <li>
                            <span className="tf-nc rainbow rainbow-0">{tag.name}</span>
                            {tag.tags.length > 0 && getBranch(tag.tags, 0)}
                        </li>
                    </ul>
                </div>
            );
        })
    );


    results.push(
        <div className="tf-tree">
            <ul>
                {smallTags.map(tag => {
                    colourIndex = 0;
                    return (
                        <li key={tag.id}>
                            <span className={classNames("tf-nc", nextColour())}>{tag.name}</span>
                            {tag.tags.length > 0 && getBranch(tag.tags, 0)}
                        </li>
                    );
                })}
            </ul>
        </div>
    );

    return results;
}

const getBranch = (tags: Tag[], level: number): ReactNode => {

    return (
        <ul>
            {tags.map(tag => (
                <li key={tag.id}>
                    <span className={classNames("tf-nc", level === 0 ? nextColour() : `rainbow rainbow-${colourIndex}`)}>{tag.name}</span>
                    {tag.tags.length > 0 && getBranch(tag.tags, level + 1)}
                </li>
            ))}
        </ul>
    )

}
