using _Games.CloudAPI.Model;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _Games.CloudAPI.Samples
{
    public class SamplesUploadLeaderBoard : MonoBehaviour
    {
        public int max = 10;
        public string textAsset = @"3759e58b7247eb38899ea027de662317d872d520,58ba538b-c1cc-4769-bb3d-89f25e432311,3f94684a-ab5c-4528-8569-0a78699421d9,87d8a7c7-7395-4556-84e6-af7f2f0e9037,6945a7c4-b146-4a0b-a11f-0f2380349166,88322cae-232e-4dad-9972-b3a0df3bb052,1d468792-f811-4d39-a3c8-abf64f4ceb34,a781cacc-bdd1-480b-bbff-3d770c9e6ef7,26ecb952-effb-4bf8-9fdb-5b58dd524f46,1305737b-72d7-4c4e-a324-648455acb926,09195dff-81bc-41d2-b92b-8f7fe5ecc98b,481f0fe8-ed4a-4a7f-8463-ab6ae8a84fcd,1c425896-08f1-461c-b219-49d9211d34ac,21ead451-6389-45e7-a78f-3cfffd2a131d,bbcae5fc-8bcb-4a60-a4d1-845293e67506,7a1fb33b-cccf-4983-88e3-715e67ea8b27,d022ac5b-6b04-45d2-a048-d2498ecbb95e,0855b063-fce2-47c9-a057-b415eee16f56,ce768d6d-cd46-462c-9dd2-2fb6385ea6d7,8b35a133-e012-4d66-b79b-b3ac3f1bb8c9,63788ed6-2cc0-4eaf-86bd-5d5edd46a53c,c3c8eb75-8c42-41fb-9c9e-78aed629f93c,6a2d5396-26cd-45d6-8676-6e9f75045180,199458e9-8f59-4169-9a30-6b2bdf34a5c4,8a3171e6-48e8-46d3-a92f-21aebe799481,8ed4299c-a545-4aa4-a2d5-04875d588889,47918a7a-8c9c-4340-a529-f98731e127db,c566f7e8-a7b9-4edc-b9b7-9787aec0acf2,0e3973f4-0e6d-4e9c-8bd6-c6a0d523041e,fce70277-db2e-4b04-a676-f39d91fa5bd2,660e9fde-07c3-44c7-a96f-84914ac574f3,638e78ae-ef54-4899-aed7-448f79dc8f25,497ebd27-1420-4ed6-af62-2be8f00d0dfa,fab44097-6a36-441d-a78e-7eedb5c5e232,9bc4d4fc-8b99-4df1-96de-6414d05cbc8f,9d12e270-36db-4d97-90ce-ec96f48fafe0,06659427-297a-45b6-a296-5457e227c759,41e1da34-57bd-4df8-a937-f645f15e13b3,f3ca8024-8cb7-4438-8818-87426fbfb318,08a87ec8-441f-46a2-bf5b-58af725084d4,72ffc7e8-52f4-43ae-916f-ccff659a5ae6,d03cd3fa-f831-4642-b1ab-42260326208a,649a8a66-bcbe-4472-a1c2-fce99e169c3c,55bb8363-e0b4-4347-9a93-cbfc80f246a8,33d9b9f1-3b79-4b0e-b89d-bd8a705a7899,df5b3460-b940-4e6c-bd3e-d8283d866833,1a6f45f6-64db-4163-b38e-745da69b097e,ca86546a-a6e9-4e42-a252-4a0ac1355eb8,13f9c6db-2e7a-4134-9b0c-aa304ea764c5,2198e3e8-a381-457f-85cc-3efb4429789c,6c5f941d-41b3-46af-b46e-7987891cfff7";

        private async void Start()
        {
            Application.runInBackground = true;
            System.Random random = new System.Random();
            string[] usersId = textAsset.Split(',');
            int count = Mathf.Min(max, usersId.Length);
            for (int i = 0; i < count; i++)
            {
                int score = random.Next(100, 10000);
                UploadData(usersId[i], score);
                await UniTask.WaitForSeconds(10);
            }
        }

        async void UploadData(string userId, int score)
        {
            RequestResult login = (await CloudAPIUtils.LoginWithId(userId)).result;
            if (!login.success)
            {
                Debug.LogError($"Error login: {login.message}, code: {login.errorCode}");
                return;
            }

            var joinLeaderBoardResult = await CloudAPIUtils.JoinLeaderboard();
            if (joinLeaderBoardResult.result.success)
            {
                Debug.Log($"Successfully joined leaderboard: {JsonUtility.ToJson(joinLeaderBoardResult.rankResult)}");
            }
            else
            {
                Debug.LogError($"Error join leaderboard: {joinLeaderBoardResult.result.message}");
            }
            
            var updateLeaderBoardResult = await CloudAPIUtils.UpdateLeaderBoard(new PlayerScore(score));
            if (updateLeaderBoardResult.success)
            {
                Debug.Log("update leaderboard success.");
            }
            else
            {
                Debug.LogError("update leaderboard failed. " + updateLeaderBoardResult.message + ". Code: " + updateLeaderBoardResult.errorCode);
            }

            CloudAPIUtils.Logout();
        }
    }
}