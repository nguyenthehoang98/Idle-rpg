export default function FileUpload({ onFile }: any) {
  return (
    <input
      type="file"
      onChange={(e) => onFile(e.target.files?.[0])}
    />
  );
}