import { ArrowRight, Mail, Plus } from "lucide-react";
import { Button } from "./components/Button";

function App() {
  return (
    <div className="flex flex-col gap-8 p-8">
      <div className="space-y-4">
        <h2 className="text-xl font-bold">Button Variants</h2>
        <div className="flex flex-wrap gap-4">
          <Button variant="primary">Primary</Button>
          <Button variant="secondary">Secondary</Button>
          <Button variant="tertiary">Tertiary</Button>
          <Button variant="outline">Outline</Button>
          <Button variant="error">Error</Button>
          <Button variant="link">Link</Button>
        </div>
      </div>

      <div className="space-y-4">
        <h2 className="text-xl font-bold">Button Sizes</h2>
        <div className="flex flex-wrap items-center gap-4">
          <Button size="sm">Small</Button>
          <Button size="md">Medium</Button>
          <Button size="lg">Large</Button>
        </div>
      </div>

      <div className="space-y-4">
        <h2 className="text-xl font-bold">With Icons</h2>
        <div className="flex flex-wrap gap-4">
          <Button icon={<Mail />}>Email</Button>
          <Button icon={<Plus />} variant="secondary">
            Add New
          </Button>
          <Button icon={<ArrowRight />} iconPosition="right" variant="outline">
            Next Step
          </Button>
        </div>
      </div>

      <div className="space-y-4">
        <h2 className="text-xl font-bold">States</h2>
        <div className="flex flex-wrap gap-4">
          <Button disabled>Disabled</Button>
        </div>
      </div>
    </div>
  );
}

export default App;
